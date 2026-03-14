using StudentService.DTOs;
using StudentService.Entities;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;

namespace StudentService.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;

        public StudentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public async Task CreateProfileAsync(Guid userId, CreateStudentProfileDto dto)
        {
            if (dto.Year is < 1 or > 4)
                throw new ArgumentException("Invalid year");

            if (dto.CGPA is < 0 or > 10)
                throw new ArgumentException("Invalid CGPA");

            if (await _repo.ExistsByUserIdAsync(userId))
                throw new InvalidOperationException("Profile already exists");

            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RollNo = dto.RollNo,
                EnrollmentNo = dto.EnrollmentNo,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Course = dto.Course,
                Branch = dto.Branch,
                Year = dto.Year,
                CGPA = dto.CGPA,
                Skills = dto.Skills.Select(skill => new StudentSkill
                {
                    Id = Guid.NewGuid(),
                    SkillName = skill
                }).ToList()
            };
            student.ProfileProgress = ProfileProgressCalculator.Calculate(student);
            await _repo.AddAsync(student);
            await _repo.SaveChangesAsync();
        }

        public async Task<StudentProfileResponseDto> GetProfileAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Profile not found");

            return Map(student);
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateStudentProfileDto dto)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Profile not found");

            student.FullName = dto.FullName ?? student.FullName;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Course = dto.Course ?? student.Course;
            student.Branch = dto.Branch ?? student.Branch;
            student.Year = dto.Year ?? student.Year;
            student.CGPA = dto.CGPA ?? student.CGPA;

            if (dto.Skills != null)
            {
                student.Skills.Clear();
                foreach (var skill in dto.Skills)
                {
                    student.Skills.Add(new StudentSkill
                    {
                        Id = Guid.NewGuid(),
                        SkillName = skill
                    });
                }
            }
            student.ProfileProgress = ProfileProgressCalculator.Calculate(student);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteProfileAsync(Guid studentId)
        {
            var student = await _repo.GetByIdAsync(studentId)
                ?? throw new KeyNotFoundException("Profile not found");

            await _repo.DeleteAsync(student);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<StudentProfileResponseDto>> GetAllProfilesAsync()
        {
            return (await _repo.GetAllAsync()).Select(Map).ToList();
        }

        public async Task<List<StudentProfileResponseDto>> GetProfilesInBulkAsync(List<Guid> userIds)
        {
            return (await _repo.GetByUserIdsAsync(userIds))
                .Select(Map)
                .ToList();
        }

        public async Task<Decimal> GetProfileProgressAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Profile not found");

            return student.ProfileProgress;
        }

        public async Task<ProfileCompletionDto> GetProfileCompletionStatus(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Profile not found");

            return new ProfileCompletionDto
            {
                AcademicInfoCompleted =
                    !string.IsNullOrEmpty(student.RollNo) &&
                    !string.IsNullOrEmpty(student.EnrollmentNo) &&
                    !string.IsNullOrEmpty(student.Course) &&
                    !string.IsNullOrEmpty(student.Branch),

                SkillsCompleted = student.Skills.Any(),

                ContactCompleted =
                    !string.IsNullOrEmpty(student.Email) &&
                    !string.IsNullOrEmpty(student.PhoneNumber),

                ResumeUploaded = student.Documents.Any(d => d.DocumentType == "Resume"),

                Progress = student.ProfileProgress
            };
        }

        private static StudentProfileResponseDto Map(Student student)
        {
            return new StudentProfileResponseDto
            {
                Id = student.Id,
                RollNo = student.RollNo,
                EnrollmentNo = student.EnrollmentNo,
                FullName = student.FullName,
                PhoneNumber = student.PhoneNumber,
                Course = student.Course,
                Branch = student.Branch,
                Year = student.Year,
                CGPA = student.CGPA,
                Skills = student.Skills.Select(s => s.SkillName).ToList()
            };
        }
    }
}
