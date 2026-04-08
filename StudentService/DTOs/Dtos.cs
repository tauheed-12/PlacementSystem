using System.ComponentModel.DataAnnotations;

namespace StudentService.DTOs
{
    public class Dtos
    {
        public record CreateStudentProfileRequest(
            [Required]
            [RegularExpression(@"^\d{2}[A-Z]{2}\d{4}$", ErrorMessage = "RollNo must follow format: 22CS1234")]
            string RollNo,

            [Required]
            [RegularExpression(@"^\d{11}$", ErrorMessage = "EnrollmentNo must be 11 digits")]
            string EnrollmentNo,

            [Required]
            [MinLength(2)]
            [MaxLength(100)]
            string FullName,

            [Required]
            [Phone]
            [RegularExpression(@"^\+?[1-9]\d{9,14}$", ErrorMessage = "Invalid phone number format")]
            string PhoneNumber,

            [Required]
            [MaxLength(100)]
            string Course,

            [Required]
            [MaxLength(100)]
            string Branch,

            [Range(1, 5, ErrorMessage = "Year must be between 1 and 5")]
            int Year,

            [Range(0.0, 10.0, ErrorMessage = "CGPA must be between 0.0 and 10.0")]
            decimal CGPA,

            [Required]
            [MinLength(1, ErrorMessage = "At least one skill is required")]
            List<string> Skills);


        public record UpdateStudentProfileRequest(
            [MinLength(2)]
            [MaxLength(100)]
            string? FullName,

            [Phone]
            [RegularExpression(@"^\+?[1-9]\d{9,14}$", ErrorMessage = "Invalid phone number format")]
            string? PhoneNumber,

            [MaxLength(100)]
            string? Course,

            [MaxLength(100)]
            string? Branch,

            [Range(1, 5, ErrorMessage = "Year must be between 1 and 5")]
            int? Year,

            [Range(0.0, 10.0, ErrorMessage = "CGPA must be between 0.0 and 10.0")]
            decimal? CGPA,

            List<string>? Skills);


        // Response records don't need validation — they are outbound data
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