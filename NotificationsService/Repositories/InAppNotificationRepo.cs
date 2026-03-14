using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;
using NotificationsService.Entites;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Repositories
{
    public class InAppNotificationRepo : IInAppNotificationRepo
    {
        private readonly NotificationDbContext _notificationDbContext;
        private readonly ILogger<InAppNotificationRepo> _logger;
        public InAppNotificationRepo(NotificationDbContext notificationDbContext, ILogger<InAppNotificationRepo> logger)
        {
            _notificationDbContext = notificationDbContext;
            _logger = logger;
        }


        public async Task AddAsync(InAppNotification inAppNotification)
        {
            if (inAppNotification == null)
            {
                _logger.LogError("InAppNotification object is null.");
                throw new ArgumentNullException(nameof(inAppNotification));
            }
            await _notificationDbContext.InAppNotifications.AddAsync(inAppNotification);
            await _notificationDbContext.SaveChangesAsync();
            _logger.LogInformation("InAppNotification added successfully with Id: {Id}", inAppNotification.Id);
        }


        public async Task<List<InAppNotification>> GetNotificationsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("UserId is empty.");
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            }
            var notifications = await _notificationDbContext.InAppNotifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} notifications for UserId: {UserId}", notifications.Count, userId);
            return notifications;
        }


        public async Task MarkAsReadAsync(Guid notificationId)
        {
            if (notificationId == Guid.Empty)
            {
                _logger.LogError("NotificationId is empty.");
                throw new ArgumentException("NotificationId cannot be empty.", nameof(notificationId));
            }
            var notification = await _notificationDbContext.InAppNotifications.FindAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("InAppNotification with Id: {Id} not found.", notificationId);
                return;
            }
            notification.IsRead = true;
            _notificationDbContext.InAppNotifications.Update(notification);
            await _notificationDbContext.SaveChangesAsync();
            _logger.LogInformation("InAppNotification with Id: {Id} marked as read.", notificationId);
        }


        public async Task DeleteAsync(Guid notificationId)
        {
            if (notificationId == Guid.Empty)
            {
                _logger.LogError("NotificationId is empty.");
                throw new ArgumentException("NotificationId cannot be empty.", nameof(notificationId));
            }
            var notification = await _notificationDbContext.InAppNotifications.FindAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("InAppNotification with Id: {Id} not found.", notificationId);
                return;
            }
            _notificationDbContext.InAppNotifications.Remove(notification);
            await _notificationDbContext.SaveChangesAsync();
            _logger.LogInformation("InAppNotification with Id: {Id} deleted successfully.", notificationId);
        }
    }
}
