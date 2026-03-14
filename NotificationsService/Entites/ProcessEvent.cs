using System.ComponentModel.DataAnnotations;

namespace NotificationsService.Entites
{
    public class ProcessEvent
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
