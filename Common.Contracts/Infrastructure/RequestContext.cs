namespace Common.Contracts.Infrastructure
{
    public class RequestContext
    {
        public Guid UserId { get; init; }
        public List<string> Roles { get; init; } = new();
        public string EmailId { get; init; } = string.Empty;

        public bool IsInRole(string role) =>
            Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

        public bool HasAnyRole(params string[] roles) =>
            roles.Any(IsInRole);
    }

}
