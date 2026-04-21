using AuthService.DTOs;
using AuthService.Services.Interfaces;
using Common.Contracts.Web;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

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

        private async Task<IActionResult?> ValidateAsync<T>(T model, IValidator<T> validator, CancellationToken ct)
        {
            var result = await validator.ValidateAsync(model, ct);
            if (!result.IsValid)
                return BadRequest(ApiEnvelope<object>.Fail(
                    "Validation failed",
                    result.Errors.Select(e => e.ErrorMessage)));

            return null;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] IValidator<RegisterRequest> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(request, validator, HttpContext.RequestAborted);
            if (error != null) return error;

            await _service.RegisterAsync(request, ct);
            return Ok(ApiEnvelope<object>.Ok("Registered successfully!, Please verify your email"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IValidator<LoginRequest> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(request, validator, HttpContext.RequestAborted);
            if (error != null) return error;

            var res = await _service.LoginAsync(request, ct);
            SetRefreshTokenCookie(res.RefreshToken);
            return Ok(ApiEnvelope<object>.Ok("Login successful", new { accessToken = res.AccessToken }));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token, [FromServices] IValidator<string> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(token, validator, HttpContext.RequestAborted);
            if (error != null) return error;

            await _service.VerifyEmailAsync(token, ct);
            return Ok(ApiEnvelope<object>.Ok("Email verified successfully!"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot([FromBody] ForgotPasswordRequest request, [FromServices] IValidator<ForgotPasswordRequest> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(request, validator, HttpContext.RequestAborted);
            if (error != null) return error;
        
            await _service.ForgotPasswordAsync(request, ct);
            return Ok(ApiEnvelope<object>.Ok("Verification mail sent successfully"));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset([FromBody] ResetPasswordRequest request, [FromServices] IValidator<ResetPasswordRequest> validator, CancellationToken ct)
        {
            var error = await ValidateAsync(request, validator, HttpContext.RequestAborted);
            if (error != null) return error;

            await _service.ResetPasswordAsync(request, ct);
            return Ok(ApiEnvelope<object>.Ok("Password reset success!"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiEnvelope<object>.Fail("Refresh token is required."));

            var token = await _service.RefreshTokenAsync(refreshToken, ct);
            return Ok(ApiEnvelope<object>.Ok("Token refreshed", new { accessToken = token }));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var token))
                await _service.LogoutAsync(token, ct);
                
            Response.Cookies.Delete("refreshToken");
            return Ok(ApiEnvelope<object>.Ok("Logout successfully"));
        }

        private void SetRefreshTokenCookie(string refreshToken, int daysValid = 7)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(daysValid)
            });
        }
    }
}