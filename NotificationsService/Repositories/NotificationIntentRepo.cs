using NotificationsService.Data;
using NotificationsService.Entites;
using NotificationsService.Entities;
using NotificationsService.Repositories.Interfaces;

public class NotificationIntentRepo : INotificationIntentRepo
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationIntentRepo> _logger;

    public NotificationIntentRepo(NotificationDbContext context, ILogger<NotificationIntentRepo> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(NotificationIntent intent)
    {
        await _context.NotificationIntents.AddAsync(intent);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Notification Intent Created {intentId}", intent.IntentId);
    }

    public async Task DeleteAsync(Guid intentId)
    {
        var existing = await _context.NotificationIntents.FindAsync(intentId);

        if (existing == null)
        {
            _logger.LogError("Notification Intent with id {intentId} not found for deletion", intentId);
            return;
        }

        _context.NotificationIntents.Remove(existing);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Notification Intent Deleted {intentId}", intentId);
    }

    public async Task<bool> IntentExists(Guid intentId)
    {
        return await _context.NotificationIntents.FindAsync(intentId) != null;
    }

    public async Task<bool> IsEventProcessed(Guid eventId)
    {
        return await _context.ProcessEvents.FindAsync(eventId) != null;
    }

    public async Task MarkEventAsProcessed(Guid eventId)
    {
        var processEvent = new ProcessEvent
        {
            EventId = eventId,
            ProcessedAt = DateTime.UtcNow
        };
        await _context.ProcessEvents.AddAsync(processEvent);
        await _context.SaveChangesAsync();
    }
}