using AuthService.DTOs;

namespace AuthService.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request, CancellationToken ct);
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
        Task VerifyEmailAsync(string token, CancellationToken ct);
        Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct);
        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);
        Task<string> RefreshTokenAsync(string refreshToken, CancellationToken ct);
        Task LogoutAsync(string refreshToken, CancellationToken ct);
    }

}
