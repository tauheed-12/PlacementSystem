using Microsoft.EntityFrameworkCore;
using AuthService.Entities;

namespace AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(64).HasColumnType("bytea");
                entity.Property(u => u.PasswordSalt).IsRequired().HasMaxLength(64).HasColumnType("bytea");
                entity.Property(u => u.IsActive).HasDefaultValue(true);
                entity.Property(u => u.IsEmailVerified).HasDefaultValue(false);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(r => r.Name).IsUnique();
                entity.HasData(
                    new Role { Id = 1, Name = "Student" },
                    new Role { Id = 2, Name = "PlacementCoordinator" },
                    new Role { Id = 3, Name = "TPO" },
                    new Role { Id = 4, Name = "Recruiter" },
                    new Role { Id = 5, Name = "Admin" }
                );
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(128);
                entity.HasIndex(rt => rt.TokenHash).IsUnique();
                entity.Property(rt => rt.ExpiresAt).IsRequired();
                entity.Property(rt => rt.IsRevoked).HasDefaultValue(false);
                entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("NOW()");
                entity.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(ut => ut.Id);
                entity.Property(ut => ut.Token).IsRequired().HasMaxLength(512);
                entity.HasIndex(ut => ut.Token).IsUnique();
                entity.Property(ut => ut.TokenType).HasConversion<string>().HasMaxLength(50);
                entity.Property(ut => ut.ExpiresAt).IsRequired();
                entity.Property(ut => ut.IsUsed).HasDefaultValue(false);
                entity.HasOne(ut => ut.User).WithMany(u => u.UserTokens).HasForeignKey(ut => ut.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(om => om.Id);
                entity.Property(om => om.EventType).IsRequired().HasMaxLength(100);
                entity.Property(om => om.Key).IsRequired().HasMaxLength(256);
                entity.Property(om => om.Payload).HasColumnType("text");
                entity.Property(om => om.IsProcessed).HasDefaultValue(false);
                entity.Property(om => om.CreatedAt).HasDefaultValueSql("NOW()");
                entity.HasIndex(om => new { om.IsProcessed, om.CreatedAt });
            });
        }
    }
}