using AuthService.Data;
using AuthService.Entities;
using AuthService.Enums;
using AuthService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        {
            return await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct)
        {
            return await _db.Users.FindAsync(userId, ct);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
        {
            return await _db.Users.AnyAsync(u => u.Email == email, ct);
        }

        public async Task AddUserAsync(User user, CancellationToken ct)
        {
            await _db.Users.AddAsync(user, ct);
        }

        public async Task AddUserRoleAsync(UserRole role, CancellationToken ct)
        {
            await _db.UserRoles.AddAsync(role, ct);
        }

        public async Task AddUserTokenAsync(UserToken token, CancellationToken ct)
        {
            await _db.UserTokens.AddAsync(token, ct);
        }

        public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct)
        {
            await _db.RefreshTokens.AddAsync(token, ct);
        }

        public async Task<UserToken?> GetValidUserTokenAsync(string token, UserTokenType type, CancellationToken ct)
        {
            return await _db.UserTokens.FirstOrDefaultAsync(t =>
                t.Token == token &&
                t.TokenType == type &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow, ct);
        }

        public void RevokeUserToken(UserToken token, CancellationToken ct)
        {
            token.IsUsed = true;
            _db.UserTokens.Update(token);
        }

        public void RevokeRefreshToken(RefreshToken token, CancellationToken ct)
        {
            token.IsRevoked = true;
            _db.RefreshTokens.Update(token);
        }

        public async Task<UserToken?> GetValidUserTokenByUserIdAsync(Guid userId, UserTokenType type, CancellationToken ct)
        {
            return await _db.UserTokens.FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.TokenType == type &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow, ct);
        }

        public async Task<RefreshToken?> GetValidRefreshTokenAsync(string hashedToken, CancellationToken ct)
        {
            return await _db.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == hashedToken &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow, ct);
        }

        public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken ct)
        {
            await _db.OutboxMessages.AddAsync(message, ct);
        }

        public async Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50, CancellationToken ct = default)
        {
            return await _db.OutboxMessages
                .Where(msg => !msg.IsProcessed)
                .OrderBy(msg => msg.CreatedAt)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
