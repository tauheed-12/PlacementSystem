namespace AuthService.Enums
{
    public enum NotificationEventType
    {
        UserRegistered,
        EmailVerified,
        PasswordResetRequested,
        PasswordResetCompleted
    }

    public enum NotificationAudience
    {
        Broadcast,
        Targeted
    }


    public enum UserTokenType
    {
        EmailVerification,
        PasswordReset
    }
}