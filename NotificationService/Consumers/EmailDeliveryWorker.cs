using NotificationService.Domain.Common;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Kafka;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Consumers
{
    public class EmailDeliveryWorker : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IEmailProvider _emailProvider;
        private readonly IUserPreferenceRepository _userPreferenceRepository;

        public EmailDeliveryWorker(IKafkaClient kafkaClient, IDeliveryRepository deliveryRepository, IEmailProvider emailProvider, IUserPreferenceRepository userPreferenceRepository)
        {
            _kafkaClient = kafkaClient;
            _deliveryRepository = deliveryRepository;
            _emailProvider = emailProvider;
            _userPreferenceRepository = userPreferenceRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var intent in _kafkaClient.Consume<NotificationIntent>("notification.deliver.email", stoppingToken))
            {
                if (_deliveryRepository.IsDelivered(intent.NotificationId, Channel.EMAIL))
                    continue;

                // fetch email address from preferences (single source of truth)
                var prefs = _userPreferenceRepository.Get(intent.UserId);
                try
                {
                    await _emailProvider.SendAsync(to: prefs.EmailAddress, subject: intent.Title, body: intent.Body);

                    // mark as delivered
                    _deliveryRepository.MarkAsDelivered(intent.NotificationId, Channel.EMAIL);
                }
                catch (Exception ex)
                {
                    _deliveryRepository.MarkAsFailed(intent.NotificationId, Channel.EMAIL, ex.Message);

                    // Throw to trigger Kafka retry
                    throw;
                }
            }

        }
    }
}
