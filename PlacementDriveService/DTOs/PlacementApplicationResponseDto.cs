using PlacementDriveService.Constants;

namespace PlacementDriveService.DTOs
{
    public class PlacementApplicationResponseDto
    {
        public Guid ApplicationId { get; set;}
        public Guid PlacementDriveId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public decimal Package { get; set; }
        public string Status { get; set; } = DriveStatus.Scheduled;
        public DateTime AppliedAt { get; set; }
    }
}
