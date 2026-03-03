using AuthService.Data;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Enums;
using AuthService.Helpers;
using AuthService.Interfaces;
using AuthService.Services;
using AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _service.RegisterAsync(dto);
            return Ok("Registered");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var res = await _service.LoginAsync(dto);
            SetRefreshTokenCookie(res.RefreshToken); 

            return Ok(new
            {
                accessToken = res.AccessToken
            });
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            await _service.VerifyEmailAsync(token);
            return Ok("Verified");
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> Forgot(ForgotPasswordDto dto)
        {
            await _service.ForgotPasswordAsync(dto);
            return Ok();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> Reset(ResetPasswordDto dto)
        {
            await _service.ResetPasswordAsync(dto);
            return Ok();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var token = await _service.RefreshTokenAsync(refreshToken!);
            return Ok(new { token });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var token))
                await _service.LogoutAsync(token);

            Response.Cookies.Delete("refreshToken");
            return Ok();
        }
        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
    }
}
