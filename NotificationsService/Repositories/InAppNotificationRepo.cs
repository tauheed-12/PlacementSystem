using Common.Contracts.Web;
using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;
using NotificationsService.Entites;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Repositories
{
    public class InAppNotificationRepo : IInAppNotificationRepo
    {
        private readonly NotificationDbContext _db;
        private readonly ILogger<InAppNotificationRepo> _logger;

        public InAppNotificationRepo(NotificationDbContext db, ILogger<InAppNotificationRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(InAppNotification notification)
        {
            await _db.InAppNotifications.AddAsync(notification);
            await _db.SaveChangesAsync();
        }

        public async Task<List<InAppNotification>> GetNotificationsAsync(Guid userId)
        {
            return await _db.InAppNotifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _db.InAppNotifications.FindAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("InAppNotification {Id} not found for MarkAsRead.", notificationId);
                throw new NotFoundException($"Notification {notificationId} not found.");
            }
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid notificationId)
        {
            var notification = await _db.InAppNotifications.FindAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("InAppNotification {Id} not found for Delete.", notificationId);
                throw new NotFoundException($"Notification {notificationId} not found.");
            }
            _db.InAppNotifications.Remove(notification);
            await _db.SaveChangesAsync();
        }
    }
}