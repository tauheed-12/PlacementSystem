namespace StudentService.Entities
{
    public class StudentDocument
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string DocumentType { get; set; } = null!; 
        public string DocumentUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Student Student { get; set; } = null!;
    }
}
