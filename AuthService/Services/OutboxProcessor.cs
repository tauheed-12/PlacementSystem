using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;

namespace AuthService.Services
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        public OutboxProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessor> logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var kafka = scope.ServiceProvider.GetRequiredService<IKafkaService>();
                
                var messages = await repo.GetUnProcessedOutboxMessagesAsync();
                
                if (messages == null || !messages.Any())
                {
                    _logger.LogInformation("No unprocessed outbox messages. Sleeping...");
                }
                
                else
                {
                    foreach (var msg in messages)
                    {
                        try
                        {
                            await kafka.PublishRaw("notifications.events", msg.Key, msg.Payload);
                            msg.IsProcessed = true;
                            msg.ProcessedAt = DateTime.UtcNow;
                            _logger.LogInformation("Processed outbox message {MessageId}", msg.Id);
                        }
                        catch (Exception ex)
                        {
                        // Don't mark as processed — it stays in outbox for retry
                        _logger.LogError(ex, "Failed to publish message {MessageId}, will retry", msg.Id);
                        }
                    }
                    
                    await repo.SaveChangesAsync(ct); // only saves the ones marked IsProcessed = true
                    _logger.LogInformation("Outbox batch complete.");
                    }
        }
        catch (OperationCanceledException)
        {
            break; // graceful shutdown, don't log as error
        }
        catch (Exception ex)
        {
            // DB down, scope resolution failed, etc. — log and retry after delay
            _logger.LogError(ex, "OutboxProcessor encountered an error. Retrying in 5s...");
        }

        await Task.Delay(5000, ct);
        }
    }
    }
}
