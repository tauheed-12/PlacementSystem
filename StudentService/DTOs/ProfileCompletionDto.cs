namespace StudentService.DTOs
{
    public class ProfileCompletionDto
    {
        public bool AcademicInfoCompleted { get; set; }
        public bool SkillsCompleted { get; set; }
        public bool ContactCompleted { get; set; }
        public bool ResumeUploaded { get; set; }
        public decimal Progress { get; set; }
    }
}
