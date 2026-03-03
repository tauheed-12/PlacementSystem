using NotificationService.Data;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Entities;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Infrastructure.Persistence
{
    public class UserPreferenceRepository : IUserPreferenceRepository
    {
        private readonly NotificationsDbContext _dbContext;

        public UserPreferenceRepository(NotificationsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserPreferences Get(Guid userId)
        {
            var p = _dbContext.UserPreferences.Find(userId)
                ?? new Entities.UserPreferenceEntity
                {
                    InAppEnabled = true,
                    EmailEnabled = true,
                    EmailAddress = "user@email.com"
                };

            return new UserPreferences(
                p.InAppEnabled,
                p.EmailEnabled,
                p.EmailAddress
            );
        }

        public void Save(Guid userId, UserPreferences prefs)
        {
            var entity = new UserPreferenceEntity
            {
                UserId = userId,
                InAppEnabled = prefs.InAppEnabled,
                EmailEnabled = prefs.EmailEnabled,
                EmailAddress = prefs.EmailAddress
            };

            _dbContext.UserPreferences.Update(entity);
            _dbContext.SaveChanges();
        }
    }
}
