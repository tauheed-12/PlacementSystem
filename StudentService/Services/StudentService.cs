using StudentService.Entities;
using Common.Contracts.Web;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;
using static StudentService.DTOs.Dtos;

namespace StudentService.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository repo, ILogger<StudentService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task CreateProfileAsync(Guid userId, CreateStudentProfileRequest request, CancellationToken cancellationToken)
        {
            if (await _repo.ExistsByUserIdAsync(userId, cancellationToken))
                throw new ConflictException("Profile already exists for this user.");

            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RollNo = request.RollNo,
                EnrollmentNo = request.EnrollmentNo,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Course = request.Course,
                Branch = request.Branch,
                Year = request.Year,
                CGPA = request.CGPA,
                Skills = request.Skills?.Select(skill => new StudentSkill
                {
                    Id = Guid.NewGuid(),
                    SkillName = skill
                }).ToList() ?? new List<StudentSkill>()
            };

            student.ProfileProgress = ProfileProgressCalculator.Calculate(student);

            await _repo.AddAsync(student, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile created for user {UserId}", userId);
        }

        public async Task<StudentProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
        {
            var student = await _repo.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            return Map(student);
        }

        public async Task<StudentProfileShortResponse> GetProfileByIdAsync(Guid studentId, CancellationToken cancellationToken)
        {
            var student = await _repo.GetByIdAsync(studentId, cancellationToken)
                ?? throw new NotFoundException($"Profile not found for student {studentId}.");

            return MapShortDto(student);
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateStudentProfileRequest dto, CancellationToken cancellationToken)
        {
            var student = await _repo.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            student.FullName = dto.FullName ?? student.FullName;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.Course = dto.Course ?? student.Course;
            student.Branch = dto.Branch ?? student.Branch;
            student.Year = dto.Year ?? student.Year;
            student.CGPA = dto.CGPA ?? student.CGPA;

            if (dto.Skills != null)
            {
                student.Skills.Clear();
                foreach (var skill in dto.Skills.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    student.Skills.Add(new StudentSkill
                    {
                        Id = Guid.NewGuid(),
                        SkillName = skill
                    });
                }
            }

            student.ProfileProgress = ProfileProgressCalculator.Calculate(student);

            await _repo.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated for user {UserId}", userId);
        }

        public async Task DeleteProfileAsync(Guid studentId, CancellationToken cancellationToken)
        {
            var student = await _repo.GetByIdAsync(studentId, cancellationToken)
                ?? throw new NotFoundException($"Profile not found for student {studentId}.");

            await _repo.DeleteAsync(student, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile deleted for student {StudentId}", studentId);
        }

        public async Task<List<StudentProfileResponse>> GetAllProfilesAsync(CancellationToken cancellationToken)
        {
            var students = await _repo.GetAllAsync(cancellationToken);
            return students.Select(Map).ToList();
        }

        public async Task<List<StudentProfileResponse>> GetProfilesInBulkAsync(List<Guid> userIds, CancellationToken cancellationToken)
        {
            var students = await _repo.GetByUserIdsAsync(userIds, cancellationToken);
            return students.Select(Map).ToList();
        }

        public async Task<ProfileCompletionResponse> GetProfileCompletionStatusAsync(Guid userId, CancellationToken cancellationToken)
        {
            var student = await _repo.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            return new ProfileCompletionResponse(
                AcademicInfoComplete(student),
                SkillsCompleted(student),
                ContactCompleted(student),
                ResumeUploaded(student),
                student.ProfileProgress
            );
        }

        // ---------------- PRIVATE HELPERS ----------------
        private static StudentProfileShortResponse MapShortDto(Student student)
        {
            return new StudentProfileShortResponse(
                student.Id,
                student.FullName,
                student.Email,
                student.ProfileProgress,
                student.IsPlaced,
                AcademicInfoComplete(student),
                SkillsCompleted(student),
                ContactCompleted(student),
                ResumeUploaded(student)
            );
        }

        private static bool AcademicInfoComplete(Student student) =>
            !string.IsNullOrEmpty(student.RollNo) &&
            !string.IsNullOrEmpty(student.EnrollmentNo) &&
            !string.IsNullOrEmpty(student.Course) &&
            !string.IsNullOrEmpty(student.Branch);

        private static bool SkillsCompleted(Student student) =>
            student.Skills.Any();

        private static bool ResumeUploaded(Student student) =>
            student.Documents.Any(d => d.DocumentType == "Resume");

        private static bool ContactCompleted(Student student) =>
            !string.IsNullOrEmpty(student.Email) &&
            !string.IsNullOrEmpty(student.PhoneNumber);

        private static StudentProfileResponse Map(Student student)
        {
            return new StudentProfileResponse(
                student.Id,
                student.RollNo,
                student.EnrollmentNo,
                student.FullName,
                student.Email,
                student.PhoneNumber,
                student.Course,
                student.Branch,
                student.Year,
                student.CGPA,
                student.IsPlaced,
                student.Skills.Select(s => s.SkillName).ToList()
            );
        }
    }
}