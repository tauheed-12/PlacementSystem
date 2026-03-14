using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
