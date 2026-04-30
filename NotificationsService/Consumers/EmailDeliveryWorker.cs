using NotificationsService.Clients.Interfaces;
using NotificationsService.Constants;
using NotificationsService.Enums;
using NotificationsService.Events;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Consumers
{
    public class EmailDeliveryWorker : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly ILogger<EmailDeliveryWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailDeliveryWorker(ILogger<EmailDeliveryWorker> logger, IServiceScopeFactory scopeFactory, IKafkaClient kafkaClient)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _kafkaClient = kafkaClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var (delivery, consumer, result) in
                _kafkaClient.Consume<EmailDeliveryEvent>("notifications.delivery.email", "email-delivery-worker", stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<INotificationDeliveryRepo>();
                var emailClient = scope.ServiceProvider.GetRequiredService<IEmailClient>();

                try
                {
                    var isDelivered = await repo.CheckStatus(delivery.IntentId, NotificationChannel.Email);
                    if (isDelivered)
                    {
                        _kafkaClient.Commit(consumer, result);
                        continue;
                    }

                    if (delivery.Email == null)
                    {
                        _logger.LogWarning("No email for user {UserId} in delivery {DeliveryId}", delivery.UserId, delivery.DeliveryId);
                        _kafkaClient.Commit(consumer, result);
                        continue;
                    }

                    await emailClient.SendAsync(delivery.Email, delivery.Title, delivery.Body);
                    await repo.UpdateStatus(delivery.IntentId, NotificationChannel.Email, DeliveryStatus.Delivered);
                    _kafkaClient.Commit(consumer, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email for delivery {DeliveryId}", delivery.DeliveryId);

                    var retryCount = await repo.GetRetryCount(delivery.IntentId, NotificationChannel.Email);

                    if (retryCount < RetryCount.MaxRetryCount)
                        await repo.UpdateRetryCount(delivery.IntentId, NotificationChannel.Email, retryCount + 1);
                    else
                    {
                        await repo.UpdateStatus(delivery.IntentId, NotificationChannel.Email, DeliveryStatus.Failed);
                        await _kafkaClient.Publish("notifications.delivery.email.dlq", delivery.DeliveryId.ToString(), delivery);
                    }
                }
            }
        }
    }
}