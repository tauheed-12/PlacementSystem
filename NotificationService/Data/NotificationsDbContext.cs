using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Entities;

namespace NotificationService.Data
{
    public class NotificationsDbContext : DbContext
    {
        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
        public DbSet<NotificationEntity> Notifications { get; set; }
        public DbSet<UserPreferenceEntity> UserPreferences { get; set; }
        protected void onModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
