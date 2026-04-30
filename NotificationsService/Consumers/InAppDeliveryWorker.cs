using NotificationsService.Clients.Interfaces;
using NotificationsService.Constants;
using NotificationsService.Entities;
using NotificationsService.Enums;
using NotificationsService.Events;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Consumers
{
    public class InAppDeliveryWorker : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly ILogger<InAppDeliveryWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public InAppDeliveryWorker(IKafkaClient kafkaClient, ILogger<InAppDeliveryWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _kafkaClient = kafkaClient;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var (delivery, consumer, result) in
                _kafkaClient.Consume<InAppDeliveryEvent>("notifications.delivery.inapp", "inapp-delivery-worker", stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var deliveryRepo = scope.ServiceProvider.GetRequiredService<INotificationDeliveryRepo>();
                var inAppRepo = scope.ServiceProvider.GetRequiredService<IInAppNotificationRepo>();

                try
                {
                    var isDelivered = await deliveryRepo.CheckStatus(delivery.IntentId, NotificationChannel.InApp);
                    if (isDelivered)
                    {
                        _kafkaClient.Commit(consumer, result);
                        continue;
                    }

                    await inAppRepo.AddAsync(new InAppNotification
                    {
                        Id = Guid.NewGuid(),
                        UserId = delivery.UserId,
                        Title = delivery.Title,
                        Body = delivery.Body,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                    await deliveryRepo.UpdateStatus(delivery.IntentId, NotificationChannel.InApp, DeliveryStatus.Delivered);
                    _kafkaClient.Commit(consumer, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing in-app delivery {DeliveryId}", delivery.DeliveryId);

                    var retryCount = await deliveryRepo.GetRetryCount(delivery.IntentId, NotificationChannel.InApp);

                    if (retryCount < RetryCount.MaxRetryCount)
                        await deliveryRepo.UpdateRetryCount(delivery.IntentId, NotificationChannel.InApp, retryCount + 1);
                    else
                        await deliveryRepo.UpdateStatus(delivery.IntentId, NotificationChannel.InApp, DeliveryStatus.Failed);
                }
            }
        }
    }
}