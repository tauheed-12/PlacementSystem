using AuthService.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public record LoginRequest(
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        string Email,

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        string Password);

    public record RegisterRequest(
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        string Email,

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RoleId must be a valid positive integer")]
        int RoleId,

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must have uppercase, lowercase, digit, and special character")]
        string Password,

        [Required]
        string ConfirmPassword);

    public record ForgotPasswordRequest(
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        string Email);

    public record ResetPasswordRequest(
        [Required]
        [MinLength(1, ErrorMessage = "Token cannot be empty")]
        string Token,

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must have uppercase, lowercase, digit, and special character")]
        string NewPassword,

        [Required]
        string ConfirmNewPassword);

    // Outbound / internal — no validation needed
    public record LoginResponse(string AccessToken, string RefreshToken);
    public record VerifyEmailEvent(Guid EventId, EventType EventType, AudienceType AudienceType, Dictionary<string, string> Data);
    public record ApiResponse<T>(bool Success, string? Message, T? Data);
    public record ApiErrorResponse(bool Success, string Message, IEnumerable<string>? Errors = null);
}