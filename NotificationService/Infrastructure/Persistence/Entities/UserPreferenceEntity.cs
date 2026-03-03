namespace NotificationService.Infrastructure.Persistence.Entities
{
    public class UserPreferenceEntity
    {
        public Guid UserId { get; set; }
        public bool InAppEnabled { get; set; }
        public bool EmailEnabled { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
    }
}
