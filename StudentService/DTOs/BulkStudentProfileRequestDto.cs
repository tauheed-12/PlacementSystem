namespace StudentService.DTOs
{
    public class BulkStudentProfileRequestDto
    {
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
