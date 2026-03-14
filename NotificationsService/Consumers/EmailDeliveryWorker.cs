using NotificationsService.Clients.Interfaces;
using NotificationsService.Constants;
using NotificationsService.Entities;
using NotificationsService.Enums;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Consumers
{
    public class EmailDeliveryWorker : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly ILogger<EmailDeliveryWorker> _logger;
        private readonly IEmailClient _emailClient;
        private readonly INotificationDeliveryRepo _deliveryRepo;
        private readonly IStudentServiceClient _studentServiceClient;

        public EmailDeliveryWorker(
            IKafkaClient kafkaClient,
            ILogger<EmailDeliveryWorker> logger,
            IEmailClient emailClient,
            INotificationDeliveryRepo deliveryRepo,
            IStudentServiceClient studentServiceClient)
        {
            _kafkaClient = kafkaClient;
            _logger = logger;
            _emailClient = emailClient;
            _deliveryRepo = deliveryRepo;
            _studentServiceClient = studentServiceClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var (delivery, consumer, result) in 
                _kafkaClient.Consume<NotificationDelivery>("notifications.delivery.email", stoppingToken))
            {
                try
                {
                    var isDelivered = await _deliveryRepo.CheckStatus(delivery.IntentId, NotificationChannel.Email);

                    if (isDelivered)
                        continue;

                    var studentEmail  = await _studentServiceClient.GetEmailByUserId(delivery.UserId);
                    if (studentEmail == null)
                    {
                        _logger.LogWarning(
                            "No email found for user {UserId} in delivery {DeliveryId}", 
                            delivery.UserId, delivery.Id);
                        continue;
                    }

                    await _emailClient.SendAsync(studentEmail, delivery.Title, delivery.Body);

                    await _deliveryRepo.UpdateStatus(
                        delivery.IntentId, 
                        NotificationChannel.Email, 
                        DeliveryStatus.Delivered);

                    _kafkaClient.Commit(consumer, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email for delivery {DeliveryId}", delivery.Id);

                    var retryCount = await _deliveryRepo.GetRetryCount(delivery.IntentId, NotificationChannel.Email);

                    if (retryCount < RetryCount.MaxRetryCount)
                    {
                        await _deliveryRepo.UpdateRetryCount(
                            delivery.IntentId, 
                            NotificationChannel.Email, 
                            retryCount + 1);
                    }
                    else
                    {
                        await _deliveryRepo.UpdateStatus(
                            delivery.IntentId, 
                            NotificationChannel.Email,
                            DeliveryStatus.Failed );

                        await _kafkaClient.Publish( "notifications.delivery.email.dlq", delivery.Id.ToString(), delivery);
                    }
                }
            }
        }
    }
}