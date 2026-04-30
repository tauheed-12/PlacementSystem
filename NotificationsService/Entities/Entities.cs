using NotificationsService.Enums;

namespace NotificationsService.Entities;

public class InAppNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class NotificationDelivery
{
    public Guid Id { get; set; }
    public Guid IntentId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public DeliveryStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationIntent
{
    public Guid IntentId { get; set; }  
    public Guid UserId { get; set; }
    public NotificationEventType EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProcessEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class UserNotificationPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailEnabled { get; set; }
    public bool InAppEnabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}