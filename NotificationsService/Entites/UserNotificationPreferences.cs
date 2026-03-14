using System.ComponentModel.DataAnnotations;

namespace NotificationsService.Entites
{
    public class UserNotificationPreferences
    {
        [Key]
        public Guid UserId { get; set; }
        public bool EmailEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
