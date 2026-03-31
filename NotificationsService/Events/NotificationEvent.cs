using NotificationsService.Enums;

namespace NotificationsService.Events
{
    public class NotificationEvent
    {
        public Guid EventId { get; set; }

        public NotificationEventType EventType { get; set; }

        public AudienceType AudienceType { get; set; }

        public List<Guid>? TargetUserIds { get; set; } // Only populated if AudienceType is Targeted

        public Dictionary<string, string>? Data { get; set; }
    }
}
