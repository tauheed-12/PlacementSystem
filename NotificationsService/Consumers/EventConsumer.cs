using NotificationsService.Clients.Interfaces;
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
        private readonly IServiceScopeFactory _scopeFactory;

        public EventConsumer(IKafkaClient kafkaClient, ILogger<EventConsumer> logger, IServiceScopeFactory scopeFactory)
        {
            _kafkaClient = kafkaClient;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var (evt, consumer, result) in
                _kafkaClient.Consume<NotificationEvent>("notifications.events", "notification-event-consumer", stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var intentRepo = scope.ServiceProvider.GetRequiredService<INotificationIntentRepo>();
                var prefsRepo = scope.ServiceProvider.GetRequiredService<IUserPreferenceRepo>();
                var deliveryRepo = scope.ServiceProvider.GetRequiredService<INotificationDeliveryRepo>();

                try
                {
                    if (await intentRepo.IsEventProcessed(evt.EventId))
                    {
                        _logger.LogInformation("Skipping duplicate event {EventId}", evt.EventId);
                        _kafkaClient.Commit(consumer, result);
                        continue;
                    }
                     
                    _logger.LogInformation("Received event {evt}", evt);

                    var email = evt.Data.TryGetValue("Email", out var e) ? e : null;
                    var link  = evt.Data.TryGetValue("Link", out var l) ? l : null;

                    if(evt.EventType == NotificationEventType.UserRegistered && string.IsNullOrEmpty(email))
                    {
                        _logger.LogWarning("UserRegistered event {EventId} missing email, skipping", evt.EventId);
                        _kafkaClient.Commit(consumer, result);
                        continue;
                    }

                    if(evt.TargetUserIds?.Any() == true)
                    {
                        foreach (var userId in evt.TargetUserIds)
                        {
                            if (!await prefsRepo.ExistsAsync(userId))
                            {
                                _logger.LogInformation("Creating default preferences for new user {UserId}", userId);
                                await prefsRepo.AddAsync(new UserNotificationPreferences
                                {
                                    UserId = userId,
                                    EmailEnabled = true,
                                    InAppEnabled = true,
                                    UpdatedAt = DateTime.UtcNow,
                                });
                            }
                        }
                        await prefsRepo.SaveChangesAsync(stoppingToken);
                    }

                    var audience = evt.AudienceType == NotificationAudience.Broadcast
                        ? await prefsRepo.GetAllAsync()
                        : evt.TargetUserIds?.Any() == true
                            ? (await prefsRepo.GetByUserIdsAsync(evt.TargetUserIds)).ToList()
                            : new List<UserNotificationPreferences>();

                    _logger.LogInformation("Processing event {EventId} for {AudienceCount} users", evt.EventId, audience.Count);

                    foreach (var prefs in audience)
                    {
                        var (title, body) = evt.EventType switch
                        {
                            NotificationEventType.UserRegistered       => ("Verify your email", $"Click to verify: {link}"),
                            NotificationEventType.EmailVerified        => ("Email Verified", $"Your email {email} is verified."),
                            NotificationEventType.PasswordResetRequested => ("Reset Password", $"Reset here: {link}"),
                            NotificationEventType.PasswordResetCompleted => ("Password Reset Successful", "Your password has been updated."),
                            _ => (null, null)
                        };

                        if (title == null || body == null) continue;

                        var intent = new NotificationIntent
                        {
                            IntentId = Guid.NewGuid(),
                            UserId = prefs.UserId,
                            EventType = evt.EventType,
                            Title = title,
                            Body = body,
                            CreatedAt = DateTime.UtcNow
                        };

                        await intentRepo.AddAsync(intent);
                        await intentRepo.SaveChangesAsync();

                        if (prefs.EmailEnabled)
                            await Publish(intent, evt, email, deliveryRepo, NotificationChannel.Email, "notifications.delivery.email");

                        if (prefs.InAppEnabled)
                            await Publish(intent, evt, email, deliveryRepo, NotificationChannel.InApp, "notifications.delivery.inapp");
                    }

                    await intentRepo.MarkEventAsProcessed(evt.EventId);
                    _kafkaClient.Commit(consumer, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventId}", evt.EventId);
                }
            }
        }

        private async Task Publish(NotificationIntent intent, NotificationEvent evt, string? email,
            INotificationDeliveryRepo repo, NotificationChannel channel, string topic)
        {
            var delivery = new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                IntentId = intent.IntentId,
                UserId = intent.UserId,
                UserEmail = email ?? string.Empty,
                Title = intent.Title,
                Body = intent.Body,
                Channel = channel,
                Status = DeliveryStatus.Pending,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(delivery);
            await repo.SaveChangesAsync();

            if (channel == NotificationChannel.Email)
            {
                await _kafkaClient.Publish(topic, delivery.Id.ToString(), new EmailDeliveryEvent
                {
                    DeliveryId = delivery.Id,
                    IntentId = intent.IntentId,
                    UserId = intent.UserId,
                    Email = email,
                    Title = intent.Title,
                    Body = intent.Body
                });
            }
            else if (channel == NotificationChannel.InApp)
            {
                await _kafkaClient.Publish(topic, delivery.Id.ToString(), new InAppDeliveryEvent
                {
                    DeliveryId = delivery.Id,
                    IntentId = intent.IntentId,
                    UserId = intent.UserId,
                    Title = intent.Title,
                    Body = intent.Body
                });
            }
        }
    }
}