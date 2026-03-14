using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PlacementDriveService.NewFolder
{
        public static class ClaimsPrincipalExtensions
        {
            public static Guid GetUserId(this ClaimsPrincipal User)
            {
                if (User?.Identity is not { IsAuthenticated: true })
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }

                // Try common claim names used for user id
                var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("id")
                          ?? User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("Missing user id claim. Expected one of: 'sub', 'nameid', 'id', 'userId'.");
                }

                if (!Guid.TryParse(userId, out var guid))
                {
                    throw new UnauthorizedAccessException($"Invalid user ID in token. Claim value: '{userId}'.");
                }

                return guid;
            }
        }
}
