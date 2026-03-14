using NotificationsService.Enums;

namespace NotificationsService.Entities
{
    public class NotificationDelivery
    {
        public Guid Id { get; set; }

        public Guid IntentId { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public NotificationChannel Channel { get; set; }

        public DeliveryStatus Status { get; set; }

        public int RetryCount { get; set; }

        public DateTime CreatedAt { get; set; }

        internal void Deconstruct(out object delivery, out object result, out object consumer)
        {
            throw new NotImplementedException();
        }
    }
}