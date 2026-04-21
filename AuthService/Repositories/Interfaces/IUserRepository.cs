using AuthService.Entities;
using AuthService.Enums;

namespace AuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct);
        Task AddUserAsync(User user, CancellationToken ct);
        Task AddUserRoleAsync(UserRole role, CancellationToken ct);
        Task AddUserTokenAsync(UserToken token, CancellationToken ct);
        Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct);
        Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken ct);
        Task<UserToken?> GetValidUserTokenAsync(string token, UserTokenType type, CancellationToken ct);
        Task<RefreshToken?> GetValidRefreshTokenAsync(string hashedToken, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        void RevokeUserToken(UserToken token, CancellationToken ct);
        void RevokeRefreshToken(RefreshToken token, CancellationToken ct);
        Task<UserToken?> GetValidUserTokenByUserIdAsync(Guid userId, UserTokenType type, CancellationToken ct);
        Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50, CancellationToken ct = default);
    }
}
