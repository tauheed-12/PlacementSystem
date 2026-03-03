namespace ApplicationService.DTO
{
    public class PlacementDriveDetailsDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public decimal Package { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> AllowedBranches { get; set; } = new List<string>();
        public DateTime DriveDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
