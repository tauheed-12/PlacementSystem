namespace NotificationsService.Enums;

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

public enum NotificationChannel
{
    InApp,
    Email,
    SMS,
    Push 
}

public enum UserNotificationPreference
{
    In,
    Disabled
}

public enum EventProcessingStatus
{
    Pending,
    Processed,
    Failed
}

public enum DeliveryStatus
{
    Pending,
    Delivered,
    Failed
}