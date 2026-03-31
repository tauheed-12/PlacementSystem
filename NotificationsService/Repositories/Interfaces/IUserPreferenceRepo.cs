using NotificationsService.Entites;

namespace NotificationsService.Repositories.Interfaces
{
    public interface IUserPreferenceRepo
    {
        public Task<UserNotificationPreferences?> GetAsync(Guid userId);
        public Task AddAsync( UserNotificationPreferences userNotificationPreferences );
        Task<IEnumerable<UserNotificationPreferences>> GetAllAsync();
        Task<IEnumerable<UserNotificationPreferences>> GetByUserIdsAsync(IEnumerable<Guid> userIds);
    }
}
