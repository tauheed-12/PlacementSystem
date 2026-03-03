using NotificationService.Domain.Notifications;

namespace NotificationService.Infrastructure.Persistence.Interfaces
{
    public interface IUserPreferenceRepository
    {
        UserPreferences Get(Guid userId);
        void Save(Guid userId, UserPreferences preferences);
    }
}
