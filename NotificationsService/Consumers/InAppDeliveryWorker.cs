using NotificationsService.Clients.Interfaces;
using NotificationsService.Constants;
using NotificationsService.Entites;
using NotificationsService.Entities;
using NotificationsService.Enums;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Consumers
{
    public class InAppDeliveryWorker : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly ILogger<InAppDeliveryWorker> _logger;
        private readonly INotificationDeliveryRepo _deliveryRepo;
        private readonly IInAppNotificationRepo _inAppNotificationRepo;
        public InAppDeliveryWorker(IKafkaClient kafkaClient, ILogger<InAppDeliveryWorker> logger, INotificationDeliveryRepo deliveryRepo, IInAppNotificationRepo inAppNotificationRepo)
        {
            _kafkaClient = kafkaClient;
            _logger = logger;
            _deliveryRepo = deliveryRepo;
            _inAppNotificationRepo = inAppNotificationRepo;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var (delivery, consumer, result) in _kafkaClient.Consume<NotificationDelivery>(
                "notifications.delivery.inapp",
                stoppingToken))
            {
                try
                {
                    var isDelivered = await _deliveryRepo.CheckStatus(delivery.IntentId, NotificationChannel.InApp);
                    if (isDelivered)
                        continue;
               
                    await _inAppNotificationRepo.AddAsync(new InAppNotification
                    {
                        Id = Guid.NewGuid(),
                        UserId = delivery.UserId,
                        Title = delivery.Title,
                        Body = delivery.Body,
                        IsRead = false,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    });

                    await _deliveryRepo.UpdateStatus(delivery.IntentId, NotificationChannel.InApp, DeliveryStatus.Delivered);
                    _kafkaClient.Commit(consumer, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing in-app delivery for delivery {DeliveryId}", delivery.Id);
                    var retryCount = await _deliveryRepo.GetRetryCount(delivery.IntentId, NotificationChannel.InApp);
                    if (retryCount < RetryCount.MaxRetryCount)
                    {
                        await _deliveryRepo.UpdateRetryCount(delivery.IntentId, NotificationChannel.InApp, retryCount + 1);
                    }
                    else
                    {
                        await _deliveryRepo.UpdateStatus(delivery.IntentId, NotificationChannel.InApp, DeliveryStatus.Failed);
                    }
                }
            }
        }
    }
}
