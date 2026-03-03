namespace NotificationService.Domain.Notifications
{
    public sealed class InAppNotification
    {
        public string NotificationId { get; } = string.Empty;
        public string UserId { get; } = string.Empty;
        public string Title { get; } = string.Empty;
        public string Body { get; } = string.Empty;
        public bool IsRead { get; private set; } = false;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public InAppNotification(string notificationId, string userId, string title, string body)
        {
            NotificationId = notificationId;
            UserId = userId;
            Title = title;
            Body = body;
        }
        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
