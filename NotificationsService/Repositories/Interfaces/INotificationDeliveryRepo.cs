using NotificationsService.Entities;
using NotificationsService.Enums;

namespace NotificationsService.Repositories.Interfaces
{
    public interface INotificationDeliveryRepo
    {
        public Task AddAsync(NotificationDelivery notificationDelivery);
        public Task DeleteAsync(Guid deliveryId);
        public Task<bool> CheckStatus(Guid intentId, NotificationChannel channel);
        public Task UpdateStatus(Guid intentId, NotificationChannel channel, DeliveryStatus status);
        public Task UpdateRetryCount(Guid intentId, NotificationChannel channel, int retryCount);
        public Task<int> GetRetryCount(Guid intentId, NotificationChannel channel);
        public Task<string> GetEmailByIntentIdAndChannel(Guid intentId, NotificationChannel channel);
        public Task SaveChangesAsync();
    }
}
