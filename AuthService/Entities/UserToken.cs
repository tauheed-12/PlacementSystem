namespace AuthService.Entities
{
    public class UserToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; } // EmailVerification, PasswordReset
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }

}
