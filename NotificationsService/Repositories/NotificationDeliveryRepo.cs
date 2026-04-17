using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;
using NotificationsService.Entities;
using NotificationsService.Enums;
using NotificationsService.Repositories.Interfaces;
using Common.Contracts.Web;

namespace NotificationsService.Repositories
{
    public class NotificationDeliveryRepo : INotificationDeliveryRepo
    {
        private readonly NotificationDbContext _db;
        private readonly ILogger<NotificationDeliveryRepo> _logger;

        public NotificationDeliveryRepo(NotificationDbContext db, ILogger<NotificationDeliveryRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAsync(NotificationDelivery notificationDelivery)
        {
            await _db.NotificationDeliveries.AddAsync(notificationDelivery);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid deliveryId)
        {
            var existing = await _db.NotificationDeliveries.FindAsync(deliveryId);
            if (existing == null)
            {
                _logger.LogWarning("Notification delivery {DeliveryId} not found for Delete.", deliveryId);
                throw new NotFoundException($"Notification delivery {deliveryId} not found.");
            }
            _db.NotificationDeliveries.Remove(existing);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CheckStatus(Guid intentId, NotificationChannel channel)
        {
            var delivery = await _db.NotificationDeliveries
                .FirstOrDefaultAsync(d => d.IntentId == intentId && d.Channel == channel);
            if (delivery == null)
            {
                _logger.LogWarning("Notification delivery not found for IntentId {IntentId} channel {Channel}.", intentId, channel);
                return false;
            }
            return delivery.Status == DeliveryStatus.Delivered;
        }

        public async Task UpdateStatus(Guid intentId, NotificationChannel channel, DeliveryStatus status)
        {
            var delivery = await _db.NotificationDeliveries
                .FirstOrDefaultAsync(d => d.IntentId == intentId && d.Channel == channel);
            if (delivery == null)
            {
                _logger.LogWarning("Notification delivery not found for IntentId {IntentId} channel {Channel}.", intentId, channel);
                throw new NotFoundException($"Notification delivery for intent {intentId} not found.");
            }
            delivery.Status = status;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRetryCount(Guid intentId, NotificationChannel channel, int retryCount)
        {
            var delivery = await _db.NotificationDeliveries
                .FirstOrDefaultAsync(d => d.IntentId == intentId && d.Channel == channel);
            if (delivery == null)
            {
                _logger.LogWarning("Notification delivery not found for IntentId {IntentId} channel {Channel}.", intentId, channel);
                throw new NotFoundException($"Notification delivery for intent {intentId} not found.");
            }
            delivery.RetryCount = retryCount;
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetRetryCount(Guid intentId, NotificationChannel channel)
        {
            var delivery = await _db.NotificationDeliveries
                .FirstOrDefaultAsync(d => d.IntentId == intentId && d.Channel == channel);
            if (delivery == null)
            {
                _logger.LogWarning("Notification delivery not found for IntentId {IntentId} channel {Channel}.", intentId, channel);
                return 0;
            }
            return delivery.RetryCount;
        }
    }
}