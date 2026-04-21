using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;

namespace AuthService.Services
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OutboxProcessor(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var kafka = scope.ServiceProvider.GetRequiredService<IKafkaService>();

                var message = await repo.GetUnProcessedOutboxMessagesAsync();

                foreach(var msg in message)
                {
                    try
                    {
                        await kafka.Publish("user-verification", msg.Key ,msg.Payload);

                        msg.IsProcessed = true;
                        msg.ProcessedAt = DateTime.UtcNow;
                    }
                    catch
                    {

                    }
                }

                await repo.SaveChangesAsync(ct);

                await Task.Delay(5000, ct);
            }
        }
    }
}
