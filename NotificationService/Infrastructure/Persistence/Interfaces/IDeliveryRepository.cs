using NotificationService.Domain.Common;
using NotificationService.Domain.Notifications;

namespace NotificationService.Infrastructure.Persistence.Interfaces
{
    public interface IDeliveryRepository
    {
        bool IsDelivered(Guid notificationId, Channel channel);
        void MarkAsDelivered(Guid notificationId, Channel channel);
        IEnumerable<InAppNotification> GetInAppNotificationsForUser(Guid userId);
        void MarkRead(Guid notificationId);
    }
}
