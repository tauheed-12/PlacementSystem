using AuthService.Enums;

namespace AuthService.DTOs
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, int RoleId, string Password, string ConfirmPassword);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmNewPassword);

    public record LoginResponse(string AccessToken, string RefreshToken);
    public record NotificationEvent{
        public Guid EventId {get; init;}
        public NotificationEventType EventType {get; init;}
        public NotificationAudience AudienceType {get; init;}
        public List<Guid>? TargetUserIds {get; init;}
        public Dictionary<string, string> Data {get; init;} = new();
    };
    public record ApiResponse<T>(bool Success, string? Message, T? Data);
    public record ApiErrorResponse(bool Success, string Message, IEnumerable<string>? Errors = null);
}