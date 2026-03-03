namespace StudentService.DTOs
{
    public class StudentProfileResponseDto
    {
        public Guid Id { get; set; } 
        public string FullName { get; set; } = null!;
        public string RollNo { get; set; } = null!;
        public string EnrollmentNo { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Course { get; set; } = null!;
        public string Branch { get; set; } = null!;
        public int Year { get; set; }
        public decimal CGPA { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }
}
