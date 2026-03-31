namespace StudentService.DTOs
{
    public class UpdateStudentProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; } 
        public string? Course { get; set; } 
        public string? Branch { get; set; } 
        public int? Year { get; set; }
        public decimal? CGPA { get; set; }
    }
}
