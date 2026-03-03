namespace NotificationService.Infrastructure.Persistence.Entities
{
    public class NotificationEntity
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public bool IsRead { get; set; }
        public bool InAppDelivered { get; set; } = false;
        public bool EmailDelivered { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }
}
