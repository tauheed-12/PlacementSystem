using NotificationsService.Entities;

namespace NotificationsService.Repositories.Interfaces
{
    public interface IUserPreferenceRepo
    {
        public Task<UserNotificationPreferences?> GetAsync(Guid userId);
        public Task AddAsync( UserNotificationPreferences userNotificationPreferences );
        Task<List<UserNotificationPreferences>> GetAllAsync();
        Task<IEnumerable<UserNotificationPreferences>> GetByUserIdsAsync(IEnumerable<Guid> userIds);
        Task<bool> ExistsAsync(Guid userId);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
