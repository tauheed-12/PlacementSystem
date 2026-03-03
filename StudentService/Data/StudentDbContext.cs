using Microsoft.EntityFrameworkCore;
using StudentService.Entities;

namespace StudentService.Data
{
    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options) {}
        // Define DbSets for your entities here
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<StudentSkill> StudentSkills { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configurations can be added here
            // Student
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.EnrollmentNo)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.RollNo)
                .IsUnique();

            // Student -> StudentSkill (1 - M)
            modelBuilder.Entity<StudentSkill>()
                .HasOne(ss => ss.Student)
                .WithMany(s => s.Skills)
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student -> StudentDocument (1 - M)
            modelBuilder.Entity<StudentDocument>()
                .HasOne(sd => sd.Student)
                .WithMany(s => s.Documents)
                .HasForeignKey(sd => sd.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
