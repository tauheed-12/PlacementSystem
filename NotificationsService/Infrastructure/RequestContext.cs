namespace DashboardOrchestrationService.Infrastructure
{
    public class RequestContext
    {
        public Guid UserId { get; init; }
        public List<string> Roles { get; init; } = [];
        public string EmailId { get; init; } = string.Empty;
        public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        public bool HasAnyRole(params string[] roles) => Roles.Any(r => IsInRole(r));
    }
}
