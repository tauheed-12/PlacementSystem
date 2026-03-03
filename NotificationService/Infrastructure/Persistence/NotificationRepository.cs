using NotificationService.Data;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Entities;
using NotificationService.Infrastructure.Persistence.Interfaces;

namespace NotificationService.Infrastructure.Persistence
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationsDbContext _dbContext;
        public NotificationRepository(NotificationsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsEventProcessed(string eventId) =>
            _dbContext.Notifications.Any(n => n.NotificationId.ToString() == eventId);

        public void SaveIntent(NotificationIntent intent)
        {
            _dbContext.Notifications.Add(new NotificationEntity
            {
                NotificationId = intent.NotificationId,
                UserId = intent.UserId,
                Title = intent.Title,
                Body = intent.Body,
                CreatedAt = DateTime.UtcNow
            });

            _dbContext.SaveChanges();
        }
    }
}
