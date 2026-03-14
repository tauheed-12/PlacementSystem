using NotificationsService.Enums;

namespace NotificationsService.Entities
{
    public class NotificationIntent
    {
        public Guid IntentId { get; set; }
        public Guid UserId { get; set; }
        public NotificationEventType EventType { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}