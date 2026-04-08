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

            // ─── User ────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                // byte[] maps to varbinary — cap at 64 bytes (HMAC-SHA256 output = 32, SHA512 = 64)
                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("varbinary(64)");

                entity.Property(u => u.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("varbinary(64)");

                entity.Property(u => u.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(u => u.IsEmailVerified)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(u => u.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ─── Role ────────────────────────────────────────────────────────────
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(r => r.Name)
                    .IsUnique();

                entity.HasData(
                    new Role { Id = 1, Name = "Student" },
                    new Role { Id = 2, Name = "PlacementCoordinator" },
                    new Role { Id = 3, Name = "TPO" },
                    new Role { Id = 4, Name = "Recruiter" },
                    new Role { Id = 5, Name = "Admin" }
                );
            });

            // ─── UserRole (junction) ─────────────────────────────────────────────
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict); // never cascade-delete roles
            });

            // ─── RefreshToken ────────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                // TokenHash, not raw token — sized for SHA-256 hex string (64 chars)
                entity.Property(rt => rt.TokenHash)
                    .IsRequired()
                    .HasMaxLength(128);   // headroom for SHA-512 hex if you switch later

                entity.HasIndex(rt => rt.TokenHash)
                    .IsUnique();          // fast lookup, collision-safe

                entity.Property(rt => rt.ExpiresAt)
                    .IsRequired();

                entity.Property(rt => rt.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(rt => rt.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(rt => rt.RevokedAt)
                    .IsRequired(false);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_RefreshToken_ExpiresAt",
                    "[ExpiresAt] > [CreatedAt]"));

                // RevokedAt only makes sense when IsRevoked = true
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_RefreshToken_RevokedAt",
                    "[IsRevoked] = 0 OR [RevokedAt] IS NOT NULL"));

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── UserToken ───────────────────────────────────────────────────────
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(ut => ut.Id);

                entity.Property(ut => ut.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.HasIndex(ut => ut.Token)
                    .IsUnique();

                // Store enum as string so DB is readable without the code
                entity.Property(ut => ut.TokenType)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(ut => ut.ExpiresAt)
                    .IsRequired();

                entity.Property(ut => ut.IsUsed)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_UserToken_ExpiresAt",
                    "[ExpiresAt] > GETUTCDATE()"));

                entity.HasOne(ut => ut.User)
                    .WithMany(u => u.UserTokens)
                    .HasForeignKey(ut => ut.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── OutboxMessage ───────────────────────────────────────────────────
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(om => om.Id);

                entity.Property(om => om.EventType)
                    .IsRequired()
                    .HasMaxLength(100);

                // Kafka partition key — keep it bounded
                entity.Property(om => om.Key)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(om => om.Payload)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");  // JSON payload, unbounded

                entity.Property(om => om.IsProcessed)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(om => om.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(om => om.ProcessedAt)
                    .IsRequired(false);

                // ProcessedAt must be set when IsProcessed = true
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_OutboxMessage_ProcessedAt",
                    "[IsProcessed] = 0 OR [ProcessedAt] IS NOT NULL"));

                // Outbox poller query: WHERE IsProcessed = 0 ORDER BY CreatedAt
                entity.HasIndex(om => new { om.IsProcessed, om.CreatedAt });
            });
        }
    }
}