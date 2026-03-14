using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApplicationService.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal? user)
        {
            if (user?.Identity is not { IsAuthenticated: true })
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("id")
                ?? user.FindFirstValue("userId");

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Missing user id claim.");
            }

            if (!Guid.TryParse(userId, out var guid))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }

            return guid;
        }
    }
}
