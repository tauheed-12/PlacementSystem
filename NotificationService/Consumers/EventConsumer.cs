using NotificationService.Domain.Common;
using NotificationService.Domain.Events;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Kafka;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Consumers
{
    public sealed class EventConsumer : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly INotificationRepository _notificationRepo;
        private readonly IDeliveryRepository _deliveryRepo;
        private readonly IUserPreferenceRepository _preferenceRepo;

        public EventConsumer(IKafkaClient kafkaClient, INotificationRepository notificationRepo, IDeliveryRepository deliveryRepo, IUserPreferenceRepository preferenceRepo)
        {
            _kafkaClient = kafkaClient;
            _notificationRepo = notificationRepo;
            _deliveryRepo = deliveryRepo;
            _preferenceRepo = preferenceRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var evt in _kafkaClient.Consume<DrivePublishEvent>("drive.events", stoppingToken))
            {
                // Idempotency check
                if (_notificationRepo.IsEventProcessed(evt.EventId))
                    continue;

                // fetch user preferences
                var userPrefs = _preferenceRepo.Get(evt.UserId);

                // create notification intent based on event and preferences
                if(evt.EventType == "DrivePublished")
                {
                    var channels = new List<Channel>();
                    if (userPrefs.EmailEnabled)
                    {
                        channels.Add(Channel.EMAIL);
                    }
                    if(userPrefs.InAppEnabled)
                    {
                        channels.Add(Channel.INAPP);
                    }

                    var intent = new NotificationIntent(
                        notificationId: evt.EventId, 
                        userId: evt.UserId,
                        title: $"New Drive Published",
                        body: $"A new drive has been published. Check it out!",
                        channels: channels
                    );

                    // Save intent and mark event as processed
                    _notificationRepo.SaveIntent(intent);

                    if(intent.RequiresInAppNotification())
                    {
                        await _kafkaClient.Publish(
                            topic: "notifications.delivery.inapp",
                            key: intent.UserId,
                            message: intent
                        );
                    }

                    if(intent.RequiresEmailNotification())
                    {
                        await _kafkaClient.Publish(
                            topic: "notifications.delivery.email",
                            key: intent.UserId,
                            message: intent
                        );
                    }

                    await _notificationRepo.MarkEventAsProcessed(evt.EventId);
                }
            }
        }
    }
}
