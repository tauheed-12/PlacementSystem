
namespace DashboardOrchestrationService.Infrastructure
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

            var userIdRaw = headers?["X-User-Id"].FirstOrDefault();
            var rolesRaw = headers?["X-User-Roles"].FirstOrDefault();
            var emailId = headers?["X-User-Email"].FirstOrDefault();

            if (!Guid.TryParse(userIdRaw, out var userId))
                throw new UnauthorizedAccessException("Missing or invalid X-User-Id header");

            if(string.IsNullOrEmpty(emailId))
                throw new UnauthorizedAccessException("Missing or invalid X-User-Email header");    

            var roles = string.IsNullOrEmpty(rolesRaw)
                ? []
                : rolesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(r => r.Trim())
                          .ToList();

            return new RequestContext { UserId = userId, Roles = roles, EmailId = emailId };
        }
    }
}
