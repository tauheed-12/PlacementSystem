using AuthService.Enums;
using System;

namespace AuthService.Entities
{
    public class UserToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserTokenType TokenType { get; set; } // e.g. "EmailVerification", "PasswordReset"
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public User? User { get; set; }
    }
}
