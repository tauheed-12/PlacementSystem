using NotificationsService.Data;
using NotificationsService.Entites;
using NotificationsService.Entities;
using NotificationsService.Enums;
using NotificationsService.Repositories.Interfaces;

public class NotificationDeliveryRepo : INotificationDeliveryRepo
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<NotificationDeliveryRepo> _logger;

    public NotificationDeliveryRepo(NotificationDbContext dbContext, ILogger<NotificationDeliveryRepo> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AddAsync(NotificationDelivery notificationDelivery)
    {
        if (notificationDelivery == null)
            throw new ArgumentNullException(nameof(notificationDelivery));

        await _dbContext.NotificationDeliveries.AddAsync(notificationDelivery);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Notification delivery created {DeliveryId}", notificationDelivery.Id);
    }

    public async Task DeleteAsync(Guid deliveryId)
    {
        var existing = await _dbContext.NotificationDeliveries.FindAsync(deliveryId);

        if (existing == null)
        {
            _logger.LogWarning("Notification delivery not found {DeliveryId}", deliveryId);
            return;
        }

        _dbContext.NotificationDeliveries.Remove(existing);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Notification delivery deleted {DeliveryId}", deliveryId);
    }

    public Task<bool> CheckStatus(Guid intentId, NotificationChannel channel)
    {
        var delivery = _dbContext.NotificationDeliveries.FirstOrDefault(d => d.IntentId == intentId && d.Channel == channel);
        if (delivery == null)
        {
            _logger.LogWarning("Notification delivery not found {IntentId} for channel {Channel}", intentId, channel);
            return Task.FromResult(false);
        }
        return Task.FromResult(delivery.Status == DeliveryStatus.Delivered);
    }

    public Task UpdateStatus(Guid intentId, NotificationChannel channel, DeliveryStatus status)
    {
        var delivery = _dbContext.NotificationDeliveries.FirstOrDefault(d => d.IntentId == intentId && d.Channel == channel);
        if (delivery == null)
        {
            _logger.LogWarning("Notification delivery not found {IntentId} for channel {Channel}", intentId, channel);
            return Task.CompletedTask;
        }
        delivery.Status = status;
        _dbContext.SaveChanges();
        _logger.LogInformation("Notification delivery status updated {IntentId} for channel {Channel} to {Status}", intentId, channel, status);
        return Task.CompletedTask;
    }

    public Task UpdateRetryCount(Guid intentId, NotificationChannel channel, int retryCount)
    {
        var delivery = _dbContext.NotificationDeliveries.FirstOrDefault(d => d.IntentId == intentId && d.Channel == channel);
        if(delivery == null)
        {
            _logger.LogWarning("Notification delivery not found {IntentId} for channel {Channel}", intentId, channel);
            return Task.CompletedTask;
        }
        delivery.RetryCount = retryCount;
        _dbContext.SaveChanges();
        _logger.LogInformation("Notification delivery retry count updated {IntentId} for channel {Channel} to {RetryCount}", intentId, channel, retryCount);
        return Task.CompletedTask;
    }

    public Task<int> GetRetryCount(Guid intentId, NotificationChannel channel)
    {
        var delivery = _dbContext.NotificationDeliveries.FirstOrDefault(d => d.IntentId == intentId && d.Channel == channel);
        if (delivery == null)
        {
            _logger.LogWarning("Notification delivery not found {IntentId} for channel {Channel}", intentId, channel);
            return Task.FromResult(0);
        }
        return Task.FromResult(delivery.RetryCount);
    }
}