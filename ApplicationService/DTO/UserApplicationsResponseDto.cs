namespace ApplicationService.DTO
{
    public class UserApplicationsResponseDto
    {
        public Guid ApplicationId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt {  get; set; }
        public DateTime DriveDate { get; set; }
    }
}
