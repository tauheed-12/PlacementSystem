// DTOs/StudentProfileDto.cs
namespace DashboardOrchestrationService.DTOs
{
    public class StudentProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal ProfileProgress { get; set; }
        public bool IsPlaced { get; set; }
        public bool IsAcademicInfoComplete { get; set; }
        public bool IsSkillsComplete { get; set; }
        public bool IsContactComplete { get; set; }
        public bool IsResumeComplete { get; set; }
    }
}