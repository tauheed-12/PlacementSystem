using NotificationService.Domain.Common;

namespace NotificationService.Domain.Notifications
{
    public sealed class DeliveryAttempt
    {
        public string NotificationId { get; } = string.Empty;
        public Channel Channel { get; }
        public DateTime AttemptedAt { get; } = DateTime.UtcNow;
        public bool IsSuccessful { get; }
        public string? FailureReason { get; }

        private DeliveryAttempt(string notificationId, Channel channel, bool isSuccessful, string? failureReason = null)
        {
            NotificationId = notificationId;
            Channel = channel;
            IsSuccessful = isSuccessful;
            FailureReason = failureReason;
        }

        public static DeliveryAttempt Succeeded(string notificationId, Channel channel)
        {
            return new DeliveryAttempt(notificationId, channel, true);
        }

        public static DeliveryAttempt Failed(string notificationId, Channel channel, string failureReason)
        {
            return new DeliveryAttempt(notificationId, channel, false, failureReason);
        }
    }
}
