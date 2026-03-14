using NotificationsService.Clients.Interfaces;
using NotificationsService.Entites;
using NotificationsService.Entities;
using NotificationsService.Enums;
using NotificationsService.Events;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Consumers
{
    public sealed class EventConsumer : BackgroundService
    {
        private readonly IKafkaClient _kafkaClient;
        private readonly ILogger<EventConsumer> _logger;
        private readonly INotificationIntentRepo _intentRepo;
        private readonly IUserPreferenceRepo _prefsRepo;
        private readonly INotificationDeliveryRepo _deliveryRepo;

        public EventConsumer(
            IKafkaClient kafkaClient,
            ILogger<EventConsumer> logger,
            INotificationIntentRepo intentRepo,
            IUserPreferenceRepo prefsRepo,
            INotificationDeliveryRepo deliveryRepo)
        {
            _kafkaClient = kafkaClient;
            _logger = logger;
            _intentRepo = intentRepo;
            _prefsRepo = prefsRepo;
            _deliveryRepo = deliveryRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var (evt, consumer, result) in _kafkaClient.Consume<NotificationEvent>("notifications.events", stoppingToken))
            {
                try
                {
                    if (await _intentRepo.IsEventProcessed(evt.EventId))
                        continue;

                    var userPrefs = await _prefsRepo.GetAsync(evt.UserId);

                    if (userPrefs == null)
                        continue;

                    await HandleDrivePublishedEvent(evt, userPrefs);

                    await _intentRepo.MarkEventAsProcessed(evt.EventId);
                    _kafkaClient.Commit(consumer, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventId}", evt.EventId);
                }
            }
        }

        private async Task HandleDrivePublishedEvent(
            NotificationEvent evt,
            UserNotificationPreferences prefs)
        {
            var company = evt.Data?.GetValueOrDefault("CompanyName") ?? "a company";

            var intent = new NotificationIntent
            {
                IntentId = Guid.NewGuid(),
                UserId = evt.UserId,
                EventType = evt.EventType,
                Title = $"New drive published by {company}",
                Body = $"A new drive has been published by {company}. Check it out!",
                CreatedAt = DateTime.UtcNow
            };

            await _intentRepo.AddAsync(intent);

            if (prefs.EmailEnabled)
            {
                var delivery = new NotificationDelivery
                {
                    Id = Guid.NewGuid(),
                    IntentId = intent.IntentId,
                    UserId = intent.UserId,
                    Title = intent.Title,
                    Body = intent.Body,
                    Channel = NotificationChannel.Email,
                    Status = DeliveryStatus.Pending,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _deliveryRepo.AddAsync(delivery);

                await _kafkaClient.Publish(
                    "notifications.delivery.email",
                    delivery.Id.ToString(),
                    delivery);
            }

            if (prefs.InAppEnabled)
            {
                var delivery = new NotificationDelivery
                {
                    Id = Guid.NewGuid(),
                    IntentId = intent.IntentId,
                    UserId = intent.UserId,
                    Title = intent.Title,
                    Body = intent.Body,
                    Channel = NotificationChannel.InApp,
                    Status = DeliveryStatus.Pending,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _deliveryRepo.AddAsync(delivery);

                await _kafkaClient.Publish(
                    "notifications.delivery.inapp",
                    delivery.Id.ToString(),
                    delivery);
            }
        }
    }
}