using StudentService.DTOs;
using StudentService.Entities;
using StudentService.Exceptions;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;
using System.Net.NetworkInformation;

namespace StudentService.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IStudentRepository repo,
            ILogger<StudentService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task CreateProfileAsync(Guid userId, CreateStudentProfileDto dto)
        {
            if (dto.Year is < 1 or > 4)
                throw new ValidationException("Year must be between 1 and 4.");

            if (dto.CGPA is < 0 or > 10)
                throw new ValidationException("CGPA must be between 0 and 10.");

            if (await _repo.ExistsByUserIdAsync(userId))
                throw new ConflictException("Profile already exists for this user.");

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

            _logger.LogInformation("Profile created for user {UserId}", userId);
        }

        public async Task<StudentProfileResponseDto> GetProfileAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            return Map(student);
        }

        public async Task<StudentProfileShortDto> GetProfileByIdAsync(Guid studentId)
        {
            var student = await _repo.GetByIdAsync(studentId)
                ?? throw new NotFoundException($"Profile not found for student {studentId}.");
            return MapShortDto(student);
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateStudentProfileDto dto)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            if (dto.Year.HasValue && dto.Year is < 1 or > 4)
                throw new ValidationException("Year must be between 1 and 4.");

            if (dto.CGPA.HasValue && dto.CGPA is < 0 or > 10)
                throw new ValidationException("CGPA must be between 0 and 10.");

            student.FullName = dto.FullName ?? student.FullName;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Course = dto.Course ?? student.Course;
            student.Branch = dto.Branch ?? student.Branch;
            student.Year = dto.Year ?? student.Year;
            student.CGPA = dto.CGPA ?? student.CGPA;

            student.ProfileProgress = ProfileProgressCalculator.Calculate(student);

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Profile updated for user {UserId}", userId);
        }

        public async Task DeleteProfileAsync(Guid studentId)
        {
            var student = await _repo.GetByIdAsync(studentId)
                ?? throw new NotFoundException($"Profile not found for student {studentId}.");

            await _repo.DeleteAsync(student);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Profile deleted for student {StudentId}", studentId);
        }

        public async Task<List<StudentProfileResponseDto>> GetAllProfilesAsync()
        {
            var students = await _repo.GetAllAsync();
            return students.Select(Map).ToList();
        }

        public async Task<List<StudentProfileResponseDto>> GetProfilesInBulkAsync(List<Guid> userIds)
        {
            var students = await _repo.GetByUserIdsAsync(userIds);
            return students.Select(Map).ToList();
        }

        public async Task<ProfileCompletionDto> GetProfileCompletionStatusAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

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

        private static StudentProfileShortDto MapShortDto(Student student)
        {

            var academicInfoCompleted = AcademicInfoComplete(student);
            var skillsCompleted = SkillsCompleted(student);
            var contactCompleted = ContactCompleted(student);
            var resumeUploaded = ResumeUploaded(student);

            return new StudentProfileShortDto
            {
                Id = student.Id,
                Name = student.FullName,
                IsPlaced = student.IsPlaced,
                ProfileProgress = student.ProfileProgress,
                IsAcademicInfoComplete = academicInfoCompleted,
                IsContactComplete = contactCompleted,
                IsResumeComplete = resumeUploaded,
                IsSkillsComplete = skillsCompleted,
            };
        }

        private static bool AcademicInfoComplete(Student student)
        {
            return !string.IsNullOrEmpty(student.RollNo) &&
                   !string.IsNullOrEmpty(student.EnrollmentNo) &&
                   !string.IsNullOrEmpty(student.Course) &&
                   !string.IsNullOrEmpty(student.Branch);
        }

        private static bool SkillsCompleted(Student student)
        {
            return student.Skills.Any();
        }

        private static bool ResumeUploaded(Student student)
        {
            return student.Documents.Any(d => d.DocumentType == "Resume");
        }

        private static bool ContactCompleted(Student student)
        {
            return !string.IsNullOrEmpty(student.Email) &&
                   !string.IsNullOrEmpty(student.PhoneNumber);
        }

        private static StudentProfileResponseDto Map(Student student)
        {
            return new StudentProfileResponseDto
            {
                Id = student.Id,
                RollNo = student.RollNo,
                EnrollmentNo = student.EnrollmentNo,
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Course = student.Course,
                Branch = student.Branch,
                Year = student.Year,
                CGPA = student.CGPA,
                IsPlaced = student.IsPlaced,
                Skills = student.Skills.Select(s => s.SkillName).ToList()
            };
        }
    }
}