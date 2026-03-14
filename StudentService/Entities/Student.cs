namespace StudentService.Entities
{
    public class Student
    {
        public Guid Id { get; set; }

        // Reference to AuthService User (NO FK to Auth DB)
        public Guid UserId { get; set; }
        public string RollNo { get; set; } = null!;
        public string EnrollmentNo { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Course { get; set; } = null!;
        public string Branch { get; set; } = null!;
        public string Batch { get; set; } = null!;
        public int Semester { get; set; } = 0;
        public int Year { get; set; }
        public decimal CGPA { get; set; }
        public bool IsPlaced { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal ProfileProgress { get; set; } = 0;

        // Navigation (inside StudentService DB only)
        public ICollection<StudentSkill> Skills { get; set; } = new List<StudentSkill>();
        public ICollection<StudentDocument> Documents { get; set; } = new List<StudentDocument>();
    }
}
