using Microsoft.EntityFrameworkCore;

namespace ApplicationService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Entities.Application> Applications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entities.Application>(b =>
            {
                b.HasKey(a => a.Id);

                b.Property(a => a.StudentUserId)
                    .IsRequired();

                b.Property(a => a.DriveId)
                    .IsRequired();

                b.Property(a => a.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property(a => a.AppliedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                b.HasIndex(a => new { a.StudentUserId, a.DriveId })
                    .IsUnique();

                b.HasIndex(a => a.StudentUserId);
                b.HasIndex(a => a.DriveId);
            });
        }
    }
}