using System.ComponentModel.DataAnnotations;

namespace ApplicationService.DTO
{
    public class ApplyRequestDto
    {
        [Required]
        public Guid DriveId { get; set; }
    }
}
