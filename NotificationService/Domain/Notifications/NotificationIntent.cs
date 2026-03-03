using NotificationService.Domain.Common;

namespace NotificationService.Domain.Notifications
{
    public sealed class NotificationIntent
    {
        public Guid NotificationId { get; }
        public Guid UserId { get; }
        public string Title { get; } = string.Empty;
        public string Body { get; } = string.Empty;
        public IReadOnlyCollection<Channel> Channels { get; } = Array.Empty<Channel>();
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public NotificationIntent(string notificationId, string userId, string title, string body, IEnumerable<Channel> channels)
        {
            NotificationId = notificationId;
            UserId = userId;
            Title = title;
            Body = body;
            Channels = channels.ToList().AsReadOnly();
        }

        public bool RequiresInAppNotification() => Channels.Contains(Channel.INAPP);
        public bool RequiresEmailNotification() => Channels.Contains(Channel.EMAIL);
    }
}
