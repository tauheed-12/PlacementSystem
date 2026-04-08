namespace AuthService.Enums
{
    public enum UserTokenType
    {
        EmailVerification = 1,
        PasswordReset = 2,
    }

    public enum AudienceType
    {
        Broadcast = 1,
        Targeted = 2
    }

    public enum EventType
    {
        UserRegistered = 1,
        PasswordResetRequested = 2,
        EmailVerified = 3,
        PasswordResetCompleted = 4
    }
}
