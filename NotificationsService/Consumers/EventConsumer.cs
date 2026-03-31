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

                    _logger.LogInformation("Received event {EventId} of type {EventType}", evt.EventId, evt.EventType);
                    IEnumerable<UserNotificationPreferences> audience = evt.AudienceType switch
                    { 
                        AudienceType.Broadcast => await _prefsRepo.GetAllAsync(),

                        AudienceType.Targeted when evt.TargetUserIds?.Count > 0 => await _prefsRepo.GetByUserIdsAsync(evt.TargetUserIds),

                        _=> Enumerable.Empty<UserNotificationPreferences>()
                    };
                    _logger.LogInformation("Processing event {EventId} for {AudienceCount} users", evt.EventId, audience.Count());  

                    foreach (var prefs in audience)
                    {
                        await HandleDrivePublishedEvent(evt, prefs);
                    }

                    await _intentRepo.MarkEventAsProcessed(evt.EventId);
                    _logger.LogInformation("Processed event {EventId} for {AudienceCount} users", evt.EventId, audience.Count());

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
            
            _logger.LogInformation("Handling drive published event {EventId} for user {UserId} with company {Company}", evt.EventId, prefs.UserId, company);
            var intent = new NotificationIntent
            {
                IntentId = Guid.NewGuid(),
                UserId = prefs.UserId,
                EventType = evt.EventType,
                Title = $"New drive published by {company}",
                Body = $"A new drive has been published by {company}. Check it out!",
                CreatedAt = DateTime.UtcNow
            };

            await _intentRepo.AddAsync(intent);
            _logger.LogInformation("Created notification intent {IntentId} for user {UserId}", intent.IntentId, prefs.UserId);

            if (prefs.EmailEnabled)
            {
                _logger.LogInformation("User {UserId} has email enabled. Creating email delivery for intent {IntentId}", prefs.UserId, intent.IntentId);
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
                _logger.LogInformation("Created email delivery {DeliveryId} for intent {IntentId} and user {UserId}", delivery.Id, intent.IntentId, prefs.UserId);

                await _kafkaClient.Publish(
                    "notifications.delivery.email",
                    delivery.Id.ToString(),
                    delivery);
                _logger.LogInformation("Published email delivery {DeliveryId} to Kafka for intent {IntentId} and user {UserId}", delivery.Id, intent.IntentId, prefs.UserId);
            }

            if (prefs.InAppEnabled)
            {
                _logger.LogInformation("User {UserId} has in-app enabled. Creating in-app delivery for intent {IntentId}", prefs.UserId, intent.IntentId);
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
                _logger.LogInformation("Created in-app delivery {DeliveryId} for intent {IntentId} and user {UserId}", delivery.Id, intent.IntentId, prefs.UserId);

                await _kafkaClient.Publish(
                    "notifications.delivery.inapp",
                    delivery.Id.ToString(),
                    delivery);
                _logger.LogInformation("Published in-app delivery {DeliveryId} to Kafka for intent {IntentId} and user {UserId}", delivery.Id, intent.IntentId, prefs.UserId);
            }
        }
    }
}