using AuthService.Data;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Helpers;
using AuthService.Interfaces;
using AuthService.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(AuthDbContext context, TokenService tokenService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if(await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already in use.");
            }

            PasswordHasher.CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new Entities.User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = dto.RoleId
            });

            var token = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                TokenType = "EmailVerification",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };
            _context.UserTokens.Add(token);

            await _context.SaveChangesAsync();

            var verificationLink = $"{_configuration["Frontend:BaseUrl"]}/verify-email?token={token.Token}";
            await _emailService.SendAsync(dto.Email, "Verify Your Email", $"Click here to verify: {verificationLink}");

            return Ok("User registered successfully.");
        }


        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            
            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt)) {
                return Unauthorized("Invalid email or password.");
            }

            if (!user.IsEmailVerified)
                return Unauthorized("Please verify your email first");


            var roles = user.UserRoles.Select(r => r.Role!.Name).ToList();
            var token = _tokenService.CreateToken(user, roles);
            return Ok(new { token });
        }


        [HttpGet("/verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var userToken = await _context.UserTokens.FirstOrDefaultAsync(ut =>
                ut.Token == token && !ut.IsUsed && ut.TokenType == "EmailVerification" && ut.ExpiresAt > DateTime.UtcNow);

            if (userToken == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            var user = await _context.Users.FindAsync(userToken.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            user.IsEmailVerified = true;
            userToken.IsUsed = true;

            await _context.SaveChangesAsync();
            return Ok("Email verified successfully.");
        }


        [HttpPost("/forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if(user == null)
            {
                return Ok();
            }

            var token = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                TokenType = "PasswordReset",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };

            _context.UserTokens.Add(token);
            await _context.SaveChangesAsync();

            var resetLink = $"{_configuration["Frontend:BaseUrl"]}/reset-password?token={token.Token}";
            await _emailService.SendAsync(dto.Email, "Reset Your Password", $"Reset your password by clicking: {resetLink}");

            return Ok("Password reset link sent");
        }


        [HttpPost("/reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var token = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.Token == dto.Token &&
                ut.TokenType == "PasswordReset" &&
                !ut.IsUsed &&
                ut.ExpiresAt > DateTime.UtcNow);

            if(token == null)
            {
                return BadRequest("Invalid or expired token");
            }

            var user = await _context.Users.FindAsync(token.UserId);
            if(user == null)
            {
                return BadRequest("User not found");
            }

            PasswordHasher.CreatePasswordHash(dto.NewPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            token.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully");
        }
    }
}
