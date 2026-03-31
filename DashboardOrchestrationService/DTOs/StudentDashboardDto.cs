// DTOs/StudentDashboardDto.cs
namespace DashboardOrchestrationService.DTOs
{
    public class StudentDashboardDto
    {
        public Guid StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal ProfileProgress { get; set; }
        public bool IsPlaced { get; set; }
        public ProfileCompletionDto ProfileCompletion { get; set; } = new();
        public DashboardStatsDto Stats { get; set; } = new();
        public List<RecentApplicationDto> RecentApplications { get; set; } = [];
        public List<string> Errors { get; set; } = [];
    }

    public class ProfileCompletionDto
    {
        public bool IsAcademicInfoComplete { get; set; }
        public bool IsSkillsComplete { get; set; }
        public bool IsContactComplete { get; set; }
        public bool IsResumeComplete { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalDrives { get; set; }
        public int Applied { get; set; }
        public int Shortlisted { get; set; }
        public int Selected { get; set; }
    }

    public class RecentApplicationDto
    {
        public Guid ApplicationId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public DateTime DriveDate { get; set; }
        public DateTime AppliedOn { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}