using AuthService.DTOs;
using AuthService.Services.Interfaces;
using Common.Contracts.Web;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _service.RegisterAsync(request);
            return Ok(ApiEnvelope<object>.Ok("Registered successfully!, Please verify your email"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var res = await _service.LoginAsync(request);
            SetRefreshTokenCookie(res.RefreshToken);
            return Ok(ApiEnvelope<object>.Ok("Login successful", new { accessToken = res.AccessToken }));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            await _service.VerifyEmailAsync(token);
            return Ok(ApiEnvelope<object>.Ok("Email verified successfully!"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot([FromBody] ForgotPasswordRequest request)
        {
            await _service.ForgotPasswordAsync(request);
            return Ok(ApiEnvelope<object>.Ok("Verification mail sent successfully"));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset([FromBody] ResetPasswordRequest request)
        {
            await _service.ResetPasswordAsync(request);
            return Ok(ApiEnvelope<object>.Ok("Password reset success!"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiEnvelope<object>.Fail("Refresh token is required."));
            var token = await _service.RefreshTokenAsync(refreshToken);
            return Ok(ApiEnvelope<object>.Ok("Token refreshed", new { accessToken = token }));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var token))
                await _service.LogoutAsync(token);
            Response.Cookies.Delete("refreshToken");
            return Ok(ApiEnvelope<object>.Ok("Logout successfully"));
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