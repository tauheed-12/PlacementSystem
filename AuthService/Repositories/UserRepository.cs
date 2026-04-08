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

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _db.Users.FindAsync(userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public async Task AddUserRoleAsync(UserRole role)
        {
            await _db.UserRoles.AddAsync(role);
        }

        public async Task AddUserTokenAsync(UserToken token)
        {
            await _db.UserTokens.AddAsync(token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            await _db.RefreshTokens.AddAsync(token);
        }

        public async Task<UserToken?> GetValidUserTokenAsync(string token, UserTokenType type)
        {
            return await _db.UserTokens.FirstOrDefaultAsync(t =>
                t.Token == token &&
                t.TokenType == type &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow);
        }

        public void RevokeUserToken(UserToken token)
        {
            token.IsUsed = true;
            _db.UserTokens.Update(token);
        }

        public void RevokeRefreshToken(RefreshToken token)
        {
            token.IsRevoked = true;
            _db.RefreshTokens.Update(token);
        }

        public async Task<UserToken?> GetValidUserTokenByUserIdAsync(Guid userId, UserTokenType type)
        {
            return await _db.UserTokens.FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.TokenType == type &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<RefreshToken?> GetValidRefreshTokenAsync(string hashedToken)
        {
            return await _db.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == hashedToken &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task AddOutboxMessageAsync(OutboxMessage message)
        {
            await _db.OutboxMessages.AddAsync(message);
        }

        public async Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50)
        {
            return await _db.OutboxMessages
                .Where(msg => !msg.IsProcessed)
                .OrderBy(msg => msg.CreatedAt)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
