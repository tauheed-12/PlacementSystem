// DTOs/ApplicationStatusResponse.cs
namespace DashboardOrchestrationService.DTOs
{
    public class ApplicationStatusResponse
    {
        public Guid Id { get; set; }
        public Guid DriveId { get; set; }
        public DateTime AppliedOn { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}