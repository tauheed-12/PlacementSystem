namespace NotificationService.Application.DTOs
{
    public sealed class PreferenceDto
    {
        public bool InAppEnabled { get; init; }
        public bool EmailEnabled { get; init; }
        public string EmailAddress { get; init; } = string.Empty;

    }
}
