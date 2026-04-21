using AuthService.Enums;

namespace AuthService.DTOs
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, int RoleId, string Password, string ConfirmPassword);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmNewPassword);

    public record LoginResponse(string AccessToken, string RefreshToken);
    public record VerifyEmailEvent(Guid EventId, EventType EventType, AudienceType AudienceType, Dictionary<string, string> Data);
    public record ApiResponse<T>(bool Success, string? Message, T? Data);
    public record ApiErrorResponse(bool Success, string Message, IEnumerable<string>? Errors = null);
}