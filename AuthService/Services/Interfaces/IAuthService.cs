using AuthService.DTOs;

namespace AuthService.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task VerifyEmailAsync(string token);

        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);

        Task<string> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }

}
