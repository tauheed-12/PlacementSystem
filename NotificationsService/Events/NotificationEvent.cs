using NotificationsService.Enums;

namespace NotificationsService.Events
{
    public class NotificationEvent
    {
        public Guid EventId { get; set; }

        public NotificationEventType EventType { get; set; }

        public Guid UserId { get; set; }

        public Dictionary<string, string> Data { get; set; } = new();
    }
}
