using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Enums;
using AuthService.Helpers;
using Common.Contracts.Web;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;
using System.Security.Cryptography;
using System.Text.Json;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IKafkaService _kafka;
        private readonly ILogger<AuthService> _logger;

        public AuthService( IUserRepository repo, TokenService tokenService, IConfiguration config, IKafkaService kafka, ILogger<AuthService> logger )
        {
            _repo = repo;
            _tokenService = tokenService;
            _config = config;
            _kafka = kafka;
            _logger = logger;
        }


        public async Task RegisterAsync(RegisterRequest request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLower();

            if (await _repo.EmailExistsAsync(email, ct))
            {
                _logger.LogError("Email {Email} is already in use", email);
                throw new ConflictException("Email already in use");
            }

            PasswordHasher.CreatePasswordHash(request.Password, out var hash, out var salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddUserAsync(user, ct);
            _logger.LogInformation("User {Email} created successfully", email);

            await _repo.AddUserRoleAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = request.RoleId,
            }, ct);
            _logger.LogInformation("User {Email} registered successfully", email);

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedToken = TokenService.HashToken(rawToken);

            var token = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = hashedToken,
                TokenType = UserTokenType.EmailVerification,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await _repo.AddUserTokenAsync(token, ct);

            var link = $"{_config["Frontend:BaseUrl"]}/verify-email?token={rawToken}";

            var message = new VerifyEmailEvent
            (
                user.Id,
                EventType.UserRegistered,
                AudienceType.Targeted,
                new Dictionary<string, string>
                {
                    { "Email", user.Email },
                    { "Link", link }
                }
            );
            var outbox = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(VerifyEmailEvent),
                Payload = JsonSerializer.Serialize(message),
                CreatedAt = DateTime.UtcNow,
                Key = user.Id.ToString(),
            };

            await _repo.AddOutboxMessageAsync(outbox, ct);
            _logger.LogInformation("User registration event add for user {Email}", email);

            await _repo.SaveChangesAsync(ct);
            _logger.LogInformation("Email verification token created for user {Email}", email);
        }


        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _repo.GetByEmailAsync(email, ct)
                ?? throw new NotFoundException("User not found");

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Invalid password attempt for user {Email}", email);
                throw new ValidationException("Invalid password");
            }

            if (!user.IsEmailVerified)
            {
                _logger.LogWarning("Login attempt with unverified email {Email}", email);
                throw new ValidationException("Email is not verified");
            }

            var roles = user.UserRoles.Select(r => r.Role!.Name).ToList();
            var accessToken = _tokenService.CreateToken(user, roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            await _repo.AddRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = TokenService.HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            }, ct);

            await _repo.SaveChangesAsync(ct);
            _logger.LogInformation("User {Email} logged in successfully", email);

            return new LoginResponse(accessToken, refreshToken);
        }


        public async Task VerifyEmailAsync(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Email verification token is missing");
                throw new BadRequestException("Token is required");
            }

            var hashedToken = TokenService.HashToken(token);
            var userToken = await _repo.GetValidUserTokenAsync(hashedToken, UserTokenType.EmailVerification, ct)
                ?? throw new NotFoundException("Invalid token");

            var user = await _repo.GetByIdAsync(userToken.UserId, ct)
                ?? throw new NotFoundException("User not found");

            user.IsEmailVerified = true;
            userToken.IsUsed = true;

            var message = new VerifyEmailEvent
                (
                    user.Id,
                    EventType.EmailVerified,
                    AudienceType.Targeted,
                     new Dictionary<string, string>
                     {
                        { "Email", user.Email }
                     }
                );

            var outboxMessage = new OutboxMessage
            {
                Id = new Guid(),
                EventType = nameof(VerifyEmailEvent),
                Payload = JsonSerializer.Serialize(message),
                CreatedAt = DateTime.UtcNow,
                Key = user.Id.ToString(),
            };

            await _repo.AddOutboxMessageAsync( outboxMessage, ct);

            _logger.LogInformation("Email verified for user {Email}", user.Email);
            await _repo.SaveChangesAsync(ct);
        }


        public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _repo.GetByEmailAsync(email, ct);

            if (user == null || !user.IsEmailVerified)
            {
                _logger.LogWarning("Password reset requested for non-existent or unverified email {Email}", email);
                return; // Don't reveal if email exists
            }

            var existingToken = await _repo.GetValidUserTokenByUserIdAsync(user.Id, UserTokenType.PasswordReset, ct);

            if (existingToken != null)
            {
                _repo.RevokeUserToken(existingToken, ct);
                await _repo.SaveChangesAsync(ct);
            }

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedToken = TokenService.HashToken(rawToken);

            var token = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = hashedToken,
                TokenType = UserTokenType.PasswordReset,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            await _repo.AddUserTokenAsync(token, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Password reset token created for user {Email}", email);

            var link = $"{_config["Frontend:BaseUrl"]}/reset-password?token={rawToken}";
           
            var message = new VerifyEmailEvent
                (
                    user.Id,
                    EventType.PasswordResetRequested,
                    AudienceType.Targeted,
                     new Dictionary<string, string>
                     {
                        { "Email", user.Email },
                        { "Link", link }
                     }
                );
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(VerifyEmailEvent),
                Payload = JsonSerializer.Serialize(message),
                CreatedAt = DateTime.UtcNow,
                Key = user.Id.ToString(),
            };

            await _repo.AddOutboxMessageAsync(outboxMessage, ct);

            _logger.LogInformation("Password reset event published for user {Email}", email);
        }


        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
        {
            var rawToken = request.Token.Trim();
            var hashedToken = TokenService.HashToken(rawToken);

            var token = await _repo.GetValidUserTokenAsync(hashedToken, UserTokenType.PasswordReset, ct)
                ?? throw new NotFoundException("Invalid token");

            var user = await _repo.GetByIdAsync(token.UserId, ct)
                ?? throw new NotFoundException("User not found");

            PasswordHasher.CreatePasswordHash(request.NewPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            token.IsUsed = true;

            var message = new VerifyEmailEvent
                (
                    user.Id,
                    EventType.PasswordResetCompleted,
                    AudienceType.Targeted,
                     new Dictionary<string, string>
                     {
                        { "Email", user.Email }
                     }
                );

            var outboxMessage = new OutboxMessage
            {
                Id = new Guid(),
                EventType = nameof(VerifyEmailEvent),
                Payload = JsonSerializer.Serialize(message),
                CreatedAt = DateTime.UtcNow,
                Key = user.Id.ToString(),
            };

            await _repo.AddOutboxMessageAsync(outboxMessage, ct);

            _logger.LogInformation("Password reset completed for user {Email}", user.Email);

            await _repo.SaveChangesAsync(ct);
        }


        public async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(refreshToken))
                throw new BadRequestException("Failed to refresh token");

            var hashed = TokenService.HashToken(refreshToken);
            var stored = await _repo.GetValidRefreshTokenAsync(hashed, ct)
                ?? throw new NotFoundException("Refresh token not found");

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            await _repo.AddRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = stored.UserId,
                TokenHash = TokenService.HashToken(newRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            }, ct);

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Refresh token rotated for user {Email}", stored.User.Email);

            var roles = stored.User.UserRoles.Select(r => r.Role!.Name).ToList();
            return _tokenService.CreateToken(stored.User, roles);
        }


        public async Task LogoutAsync(string refreshToken, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(refreshToken))
                throw new BadRequestException("Logout failed");

            var hashed = TokenService.HashToken(refreshToken);
            var token = await _repo.GetValidRefreshTokenAsync(hashed, ct);
            if (token == null) return;

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await _repo.SaveChangesAsync(ct);
            _logger.LogInformation("User {Email} logged out", token.User.Email);
        }
    }
}
