using NotificationService.Data;
using NotificationService.Domain.Common;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Interfaces;
using System.Linq;

namespace NotificationService.Infrastructure.Persistence
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly NotificationsDbContext _dbContext;

        public DeliveryRepository(NotificationsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsDelivered(Guid notificationId, Channel channel)
        {
            var n = _dbContext.Notifications.Find(notificationId);
            if (n != null)
            {
                return false;
            }
            return channel == Channel.INAPP ? n.InAppDelivered : n.EmailDelivered;
        }

        public void MarkAsDelivered(Guid notificationId, Channel channel)
        {
            var n = _dbContext.Notifications.Find(notificationId);

            if(n == null)
            {
                return;
            }

            if (channel == Channel.INAPP) n.InAppDelivered = true;
            if (channel == Channel.EMAIL) n.EmailDelivered = true;

            _dbContext.SaveChanges();
            return;
        }

        public IEnumerable<InAppNotification> GetInappNotificationsForUser(Guid userId)
        {
            return _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .Select(n => new InAppNotification(
                    n.NotificationId.ToString(),
                    n.UserId.ToString(),
                    n.Title,
                    n.Body
                ));
        }

        public void MarkRead(Guid notificationId)
        {
            var n = _dbContext.Notifications .Find(notificationId);
            n.IsRead = true;
            _dbContext.SaveChanges();
        }
    }
}
