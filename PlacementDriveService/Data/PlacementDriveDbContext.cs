using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PlacementDriveService.Entities;
using System.Text.Json;

namespace PlacementDriveService.Data
{
    public class PlacementDriveDbContext : DbContext
    {
        public PlacementDriveDbContext(DbContextOptions<PlacementDriveDbContext> options) : base(options)
        {
        }

        public DbSet<PlacementDrive> PlacementDrives { get; set; } = null!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlacementDrive>(b =>
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(p => p.JobRole)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(p => p.Description)
                    .HasMaxLength(1000);

                b.Property(p => p.Package)
                    .IsRequired()
                    .HasPrecision(10, 2); 

                b.Property(p => p.DriveDate)
                    .IsRequired();

                b.Property(p => p.ApplicationDeadline)
                    .IsRequired();

                b.Property(p => p.CreatedBy)
                    .IsRequired();

                b.Property(p => p.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                b.Property(p => p.Status)
                    .IsRequired();

                b.Property(p => p.AllowedBranches)
                 .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 )
                 .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => (c1 ?? new List<string>()).SequenceEqual(c2 ?? new List<string>()),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),
                    c => c.ToList()
                 ));

                b.HasIndex(p => p.CompanyName);
                b.HasIndex(p => p.DriveDate);
                b.HasIndex(p => p.Status);
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(om => om.Id);

                entity.Property(om => om.EventType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(om => om.Key)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(om => om.Payload)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");  

                entity.Property(om => om.IsProcessed)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(om => om.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(om => om.ProcessedAt)
                    .IsRequired(false);


                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_OutboxMessage_ProcessedAt",
                    "[IsProcessed] = 0 OR [ProcessedAt] IS NOT NULL"));

                entity.HasIndex(om => new { om.IsProcessed, om.CreatedAt });
            });
        }
    }
}