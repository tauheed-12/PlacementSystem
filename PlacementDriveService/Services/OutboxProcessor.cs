using PlacementDriveService.Repositries.Interfaces;
using PlacementDriveService.Services.Interfaces;

namespace PlacementDriveService.Services
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OutboxProcessor(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IPlacementDriveRepository>();
                var kafka = scope.ServiceProvider.GetRequiredService<IKafkaClient>();

                var message = await repo.GetUnProcessedOutboxMessagesAsync();

                foreach (var msg in message)
                {
                    try
                    {
                        await kafka.Publish("drive-notifications", msg.Key, msg.Payload);

                        msg.IsProcessed = true;
                        msg.ProcessedAt = DateTime.UtcNow;
                    }
                    catch
                    {

                    }
                }

                await repo.SaveChangesAsync();

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
