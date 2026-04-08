using AuthService.DTOs;

namespace AuthService.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task VerifyEmailAsync(string token);

        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);

        Task<string> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }

}
