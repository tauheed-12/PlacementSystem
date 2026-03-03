using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Enums;
using AuthService.Helpers;
using AuthService.Interfaces;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;
using Azure.Core;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly TokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository repo,
            TokenService tokenService,
            IEmailService emailService,
            IConfiguration config)
        {
            _repo = repo;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            dto.Email = dto.Email.Trim().ToLower();

            if (await _repo.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already in use");
           
            PasswordHasher.CreatePasswordHash(dto.Password, out var hash, out var salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddUserAsync(user);
            await _repo.AddUserRoleAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = dto.RoleId
            });

            var token = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                TokenType = UserTokenType.EmailVerification,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await _repo.AddUserTokenAsync(token);
            await _repo.SaveChangesAsync();

            var link = $"{_config["Frontend:BaseUrl"]}/verify-email?token={token.Token}";
            await _emailService.SendAsync(dto.Email, "Verify Email", link);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            dto.Email = dto.Email.Trim().ToLower();
            var user = await _repo.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException();

            if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException();

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Email not verified");

            var roles = user.UserRoles.Select(r => r.Role!.Name).ToList();
            var accessToken = _tokenService.CreateToken(user, roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            await _repo.AddRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = TokenService.HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();
            return new AuthResponseDto { 
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task VerifyEmailAsync(string token)
        {
            var userToken = await _repo.GetValidUserTokenAsync(token, UserTokenType.EmailVerification)
                ?? throw new InvalidOperationException("Invalid token");

            var user = await _repo.GetByIdAsync(userToken.UserId)
                ?? throw new InvalidOperationException();

            user.IsEmailVerified = true;
            userToken.IsUsed = true;

            await _repo.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) return;

            var token = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                TokenType = UserTokenType.PasswordReset,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            await _repo.AddUserTokenAsync(token);
            await _repo.SaveChangesAsync();

            var link = $"{_config["Frontend:BaseUrl"]}/reset-password?token={token.Token}";
            await _emailService.SendAsync(dto.Email, "Reset Password", link);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var token = await _repo.GetValidUserTokenAsync(dto.Token, UserTokenType.PasswordReset)
                ?? throw new InvalidOperationException();

            var user = await _repo.GetByIdAsync(token.UserId)
                ?? throw new InvalidOperationException();

            PasswordHasher.CreatePasswordHash(dto.NewPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            token.IsUsed = true;

            await _repo.SaveChangesAsync();
        }

        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            var hashed = TokenService.HashToken(refreshToken);
            var stored = await _repo.GetValidRefreshTokenAsync(hashed)
                ?? throw new UnauthorizedAccessException();

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            await _repo.AddRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = stored.UserId,
                TokenHash = TokenService.HashToken(newRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            var roles = stored.User.UserRoles.Select(r => r.Role!.Name).ToList();
            return _tokenService.CreateToken(stored.User, roles);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var hashed = TokenService.HashToken(refreshToken);
            var token = await _repo.GetValidRefreshTokenAsync(hashed);
            if (token == null) return;

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
        }
    }
}
