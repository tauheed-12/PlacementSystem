using System.ComponentModel.DataAnnotations;

namespace PlacementDriveService.DTOs
{
    public class PlacementDriveCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string JobRole { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Package must be non-negative")]
        public decimal Package { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public List<string> AllowedBranches { get; set; } = new List<string>();

        [Required]
        public DateTime DriveDate { get; set; }

        [Required]
        public DateTime ApplicationDeadline { get; set; }
    }
}
