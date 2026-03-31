using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PlacementDriveService.Data
{
    public class PlacementDriveDbContext : DbContext
    {
        public PlacementDriveDbContext(DbContextOptions<PlacementDriveDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.PlacementDrive> PlacementDrives { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store List<string> AllowedBranches as JSON in a single column
            modelBuilder.Entity<Entities.PlacementDrive>(b =>
            {
                b.Property(p => p.AllowedBranches)
                 .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 )
                 .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),
                    c => c.ToList()
                 ));

                // Optional: set a column type/length if desired, e.g. nvarchar(max)
                b.Property(p => p.AllowedBranches).HasColumnType("nvarchar(max)");
            });
        }
    }
}
