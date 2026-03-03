namespace NotificationService.Domain.Events
{
    public class DrivePublishEvent
    {
        public string EventId { get; set; }
        public string EventType { get; set; }
        public string UserId { get; set; }
    }
}
