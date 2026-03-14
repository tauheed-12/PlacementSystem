using Microsoft.EntityFrameworkCore;
using NotificationsService.Entites;
using NotificationsService.Entities;

namespace NotificationsService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<NotificationIntent> NotificationIntents { get; set; }
        public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
        public DbSet<UserNotificationPreferences> UserNotificationPreferences { get; set; }
        public DbSet<ProcessEvent> ProcessEvents { get; set; }
        public DbSet<InAppNotification> InAppNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
