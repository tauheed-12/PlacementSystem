namespace StudentService.Entities
{
    public class StudentSkill
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string SkillName { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
