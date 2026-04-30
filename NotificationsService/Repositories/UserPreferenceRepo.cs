using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;
using NotificationsService.Entities;
using NotificationsService.Repositories.Interfaces;

namespace NotificationsService.Repositories
{
    public class UserPreferenceRepo : IUserPreferenceRepo
    {
        private readonly NotificationDbContext _notificationDbContext;
        private readonly ILogger<UserPreferenceRepo> _logger;

        public UserPreferenceRepo(NotificationDbContext notificationDbContext, ILogger<UserPreferenceRepo> logger)
        {
            _notificationDbContext = notificationDbContext;
            _logger = logger;
        }

        public async Task<UserNotificationPreferences?> GetAsync(Guid userId)
        {
            UserNotificationPreferences? preference = await _notificationDbContext.UserNotificationPreferences.FindAsync(userId);
            return preference;
        }

        public async Task AddAsync(UserNotificationPreferences preference)
        {
            var existing = await _notificationDbContext.UserNotificationPreferences
                .FindAsync(preference.UserId);

            if (existing == null)
            {
                _logger.LogInformation("Adding new user notification preferences for user {UserId}", preference.UserId);
                await _notificationDbContext.UserNotificationPreferences.AddAsync(preference);
            }
            else
            {
                _logger.LogInformation("Updating user notification preferences for user {UserId}", preference.UserId);
                _notificationDbContext.UserNotificationPreferences.Update(preference);
            }
        }

        public async Task<List<UserNotificationPreferences>> GetAllAsync()
        {
            return await _notificationDbContext.UserNotificationPreferences.ToListAsync();
        }

        public async Task<IEnumerable<UserNotificationPreferences>> GetByUserIdsAsync(IEnumerable<Guid> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                _logger.LogWarning("GetByUserIdsAsync called with null or empty userIds");
                return Enumerable.Empty<UserNotificationPreferences>();
            }

            return await _notificationDbContext.UserNotificationPreferences
                .Where(p => userIds.Contains(p.UserId))
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _notificationDbContext.UserNotificationPreferences.AnyAsync(p => p.UserId == userId);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _notificationDbContext.SaveChangesAsync(ct);
        }
    }
}