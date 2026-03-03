namespace NotificationService.Domain.Notifications
{
    public sealed class UserPreferences
    {
        public bool InAppEnabled { get; private set; }
        public bool EmailEnabled { get; private set; }
        public string EmailAddress { get; private set; } = string.Empty;

        public UserPreferences(bool inAppEnabled, bool emailEnabled, string emailAddress)
        {
            InAppEnabled = inAppEnabled;
            EmailEnabled = emailEnabled;
            EmailAddress = emailAddress;
        }

        public void UpdateInAppPreference(bool enabled)
        {
            InAppEnabled = enabled;
        }

        public void UpdateEmailPreference(bool enabled, string emailAddress)
        {
            EmailEnabled = enabled;
            EmailAddress = emailAddress;
        }
    }
}
