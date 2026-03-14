using NotificationsService.Entites;

namespace NotificationsService.Repositories.Interfaces
{
    public interface IInAppNotificationRepo
    {
        Task AddAsync(InAppNotification inAppNotification);
        Task<List<InAppNotification>> GetNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task DeleteAsync(Guid notificationId);
    }
}
