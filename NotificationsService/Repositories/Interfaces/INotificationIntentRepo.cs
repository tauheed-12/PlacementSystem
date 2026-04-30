using NotificationsService.Entities;

namespace NotificationsService.Repositories.Interfaces
{
    public interface INotificationIntentRepo
    {
        public Task AddAsync(NotificationIntent intent);
        public Task DeleteAsync(Guid intentId);
        public Task<bool> IntentExists(Guid intentId);
        public Task<bool> IsEventProcessed(Guid eventId);
        public Task MarkEventAsProcessed(Guid eventId);
        public Task SaveChangesAsync();
    }
}
