using NotificationService.Domain.Notifications;

namespace NotificationService.Infrastructure.Persistence.Interfaces
{
    public interface INotificationRepository
    {
        bool IsEventProcessed(string eventId);
        //Task MarkEventAsProcessed(string eventId);
        void SaveIntent(NotificationIntent intent);
    }
}
