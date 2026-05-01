using Microsoft.EntityFrameworkCore;
using StudentService.Entities;

namespace StudentService.Data
{
    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<StudentSkill> StudentSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.UserId).IsRequired();
                entity.HasIndex(s => s.UserId).IsUnique();
                entity.Property(s => s.RollNo).IsRequired().HasMaxLength(20);
                entity.HasIndex(s => s.RollNo).IsUnique();
                entity.Property(s => s.EnrollmentNo).IsRequired().HasMaxLength(20);
                entity.HasIndex(s => s.EnrollmentNo).IsUnique();
                entity.Property(s => s.FullName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(s => s.Email).IsUnique();
                entity.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(s => s.Course).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Branch).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Batch).IsRequired().HasMaxLength(20);
                entity.Property(s => s.Semester).IsRequired().HasDefaultValue(0);
                entity.Property(s => s.Year).IsRequired();
                entity.Property(s => s.CGPA).IsRequired().HasColumnType("numeric(4,2)");
                entity.Property(s => s.IsPlaced).IsRequired().HasDefaultValue(false);
                entity.Property(s => s.ProfileProgress).IsRequired().HasColumnType("numeric(5,2)").HasDefaultValue(0);
                entity.Property(s => s.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<StudentSkill>(entity =>
            {
                entity.HasKey(ss => ss.Id);
                entity.Property(ss => ss.SkillName).IsRequired().HasMaxLength(100);
                entity.HasIndex(ss => new { ss.StudentId, ss.SkillName }).IsUnique();
                entity.HasOne(ss => ss.Student).WithMany(s => s.Skills).HasForeignKey(ss => ss.StudentId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StudentDocument>(entity =>
            {
                entity.HasKey(sd => sd.Id);
                entity.Property(sd => sd.DocumentType).IsRequired().HasMaxLength(50);
                entity.Property(sd => sd.DocumentUrl).IsRequired().HasMaxLength(2048);
                entity.Property(sd => sd.UploadedAt).IsRequired().HasDefaultValueSql("NOW()");
                entity.HasIndex(sd => new { sd.StudentId, sd.DocumentType }).IsUnique();
                entity.HasOne(sd => sd.Student).WithMany(s => s.Documents).HasForeignKey(sd => sd.StudentId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}