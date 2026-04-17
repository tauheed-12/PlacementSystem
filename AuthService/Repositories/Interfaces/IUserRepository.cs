using AuthService.Entities;
using AuthService.Enums;

namespace AuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid userId);
        Task<bool> EmailExistsAsync(string email);
        Task AddUserAsync(User user);
        Task AddUserRoleAsync(UserRole role);
        Task AddUserTokenAsync(UserToken token);
        Task AddRefreshTokenAsync(RefreshToken token);
        Task AddOutboxMessageAsync(OutboxMessage message);
        Task<UserToken?> GetValidUserTokenAsync(string token, UserTokenType type);
        Task<RefreshToken?> GetValidRefreshTokenAsync(string hashedToken);
        Task SaveChangesAsync();
        void RevokeUserToken(UserToken token);
        void RevokeRefreshToken(RefreshToken token);
        Task<UserToken?> GetValidUserTokenByUserIdAsync(Guid userId, UserTokenType type);
        Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50);
    }
}
