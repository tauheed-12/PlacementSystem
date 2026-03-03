using NotificationService.Domain.Common;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Kafka;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Consumers
{
    public sealed class InAppDeliveryWorker : BackgroundService
    {
        private readonly IDeliveryRepository _deliveryRepo;
        private readonly IKafkaClient _kafkaClient;

        public InAppDeliveryWorker(IDeliveryRepository deliveryRepo, IKafkaClient kafkaClient)
        {
            _deliveryRepo = deliveryRepo;
            _kafkaClient = kafkaClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var intent in _kafkaClient.Consume<NotificationIntent>("notification.delivery.inapp", stoppingToken))
            {
                // Idempotency check
                if (_deliveryRepo.IsDelivered(intent.NotificationId, Channel.INAPP))
                    continue;

                // Create in-app notification
                var inAppNotification = new InAppNotification
                (
                     notificationId: intent.NotificationId,
                     userId: intent.UserId,
                     title: intent.Title,
                     body: intent.Body
                );

                _deliveryRepo.StoreInApp(inAppNotification);

                // Mark as delivered
                _deliveryRepo.MarkAsDelivered(intent.NotificationId, Channel.INAPP);
            }
        }
    }
}
