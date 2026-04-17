using Common.Contracts.Security;
using Microsoft.AspNetCore.Http;

namespace Common.Contracts.Infrastructure
{
    public class RequestContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public RequestContext GetContext()
        {
            var headers = _httpContextAccessor.HttpContext?.Request.Headers;

            var userIdRaw = headers?[GatewayHeaders.UserId].FirstOrDefault();
            var rolesRaw = headers?[GatewayHeaders.UserRole].FirstOrDefault()
                           ?? headers?[GatewayHeaders.UserRolesLegacy].FirstOrDefault();
            var emailId = headers?[GatewayHeaders.UserEmail].FirstOrDefault();

            if (!Guid.TryParse(userIdRaw, out var userId))
                throw new UnauthorizedAccessException("Missing or invalid X-User-Id header");

            if (string.IsNullOrWhiteSpace(emailId))
                throw new UnauthorizedAccessException("Missing or invalid X-User-Email header");

            var roles = string.IsNullOrWhiteSpace(rolesRaw)
                ? new List<string>()
                : rolesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(r => r.Trim())
                          .ToList();

            return new RequestContext
            {
                UserId = userId,
                Roles = roles,
                EmailId = emailId
            };
        }
    }
}
