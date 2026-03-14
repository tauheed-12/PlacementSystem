using NotificationsService.Entites;

namespace NotificationsService.Repositories.Interfaces
{
    public interface IUserPreferenceRepo
    {
        public Task<UserNotificationPreferences?> GetAsync(Guid userId);
        public Task AddAsync( UserNotificationPreferences userNotificationPreferences );
    }
}
