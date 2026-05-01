namespace StudentService.DTOs
{
    public class Dtos
    {
        public record CreateStudentProfileRequest(string RollNo, string EnrollmentNo, string FullName, string PhoneNumber, string Course, string Branch, string Batch, int Year, decimal CGPA, List<string> Skills);
        public record UpdateStudentProfileRequest(string? FullName, string? PhoneNumber, string? Course, string? Branch, int? Year, decimal? CGPA, List<string>? Skills);
        public record AddSkillRequest(string SkillName);

        public record StudentProfileResponse(
            Guid Id,
            string RollNo,
            string EnrollmentNo,
            string FullName,
            string Email,
            string PhoneNumber,
            string Course,
            string Branch,
            int Year,
            decimal CGPA,
            bool IsPlaced,
            List<string> Skills);

        public record ProfileCompletionResponse(
            bool AcademicInfoCompleted,
            bool SkillsCompleted,
            bool ContactCompleted,
            bool ResumeUploaded,
            decimal Progress);

        public record StudentProfileShortResponse(
            Guid Id,
            string Name,
            string Email,
            decimal ProfileProgress,
            bool IsPlaced,
            bool IsAcademicInfoComplete,
            bool IsSkillsComplete,
            bool IsContactComplete,
            bool IsResumeComplete);
    }
}