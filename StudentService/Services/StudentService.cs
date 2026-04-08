using StudentService.DTOs;
using StudentService.Entities;
using StudentService.Exceptions;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;
using static StudentService.DTOs.Dtos;

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

        public async Task CreateProfileAsync(Guid userId, CreateStudentProfileRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Invalid create profile request for user {UserId}: request is null", userId);
                throw new ValidationException("Invalid request");
            }

            // Required fields validation
            if (string.IsNullOrWhiteSpace(request.RollNo))
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: RollNo is required", userId);
                throw new ValidationException("Roll number is required");
            }

            if (string.IsNullOrWhiteSpace(request.EnrollmentNo))
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: EnrollmentNo is required", userId);
                throw new ValidationException("Enrollment number is required");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: FullName is required", userId);
                throw new ValidationException("Full name is required");
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: PhoneNumber is required", userId);
                throw new ValidationException("Phone number is required");
            }

            if (string.IsNullOrWhiteSpace(request.Course))
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: Course is required", userId);
                throw new ValidationException("Course is required");
            }

            if (string.IsNullOrWhiteSpace(request.Branch))
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: Branch is required", userId);
                throw new ValidationException("Branch is required");
            }

            if (request.Year < 1 || request.Year > 4)
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: Year {Year} is out of range", userId, request.Year);
                throw new ValidationException("Year must be between 1 and 4");
            }

            if (request.CGPA < 0 || request.CGPA > 10)
            {
                _logger.LogError("CreateProfile validation failed for user {UserId}: CGPA {Cgpa} is out of range", userId, request.CGPA);
                throw new ValidationException("CGPA must be between 0 and 10");
            }

            if (await _repo.ExistsByUserIdAsync(userId))
            {
                _logger.LogWarning("Attempt to create duplicate profile for user {UserId}", userId);
                throw new ConflictException("Profile already exists for this user.");
            }

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

            await _repo.AddAsync(student);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Profile created for user {UserId}", userId);
        }

        public async Task<StudentProfileResponse> GetProfileAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            return Map(student);
        }

        public async Task<StudentProfileShortResponse> GetProfileByIdAsync(Guid studentId)
        {
            var student = await _repo.GetByIdAsync(studentId)
                ?? throw new NotFoundException($"Profile not found for student {studentId}.");
            return MapShortDto(student);
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateStudentProfileRequest dto)
        {
            if (dto == null)
            {
                _logger.LogError("Invalid update profile request for user {UserId}: request is null", userId);
                throw new ValidationException("Invalid request");
            }

            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            if (dto.Year.HasValue && (dto.Year < 1 || dto.Year > 4))
            {
                _logger.LogError("UpdateProfile validation failed for user {UserId}: Year {Year} is out of range", userId, dto.Year);
                throw new ValidationException("Year must be between 1 and 4.");
            }

            if (dto.CGPA.HasValue && (dto.CGPA < 0 || dto.CGPA > 10))
            {
                _logger.LogError("UpdateProfile validation failed for user {UserId}: CGPA {Cgpa} is out of range", userId, dto.CGPA);
                throw new ValidationException("CGPA must be between 0 and 10.");
            }

            if (dto.FullName != null && string.IsNullOrWhiteSpace(dto.FullName))
            {
                _logger.LogError("UpdateProfile validation failed for user {UserId}: FullName is empty", userId);
                throw new ValidationException("Full name cannot be empty.");
            }

            if (dto.PhoneNumber != null && string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                _logger.LogError("UpdateProfile validation failed for user {UserId}: PhoneNumber is empty", userId);
                throw new ValidationException("Phone number cannot be empty.");
            }

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
                    if (string.IsNullOrWhiteSpace(skill))
                        continue;
                    student.Skills.Add(new StudentSkill
                    {
                        Id = Guid.NewGuid(),
                        SkillName = skill
                    });
                }
            }

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

        public async Task<List<StudentProfileResponse>> GetAllProfilesAsync()
        {
            var students = await _repo.GetAllAsync();
            return students.Select(Map).ToList();
        }

        public async Task<List<StudentProfileResponse>> GetProfilesInBulkAsync(List<Guid> userIds)
        {
            if (userIds == null)
                throw new ValidationException("UserIds list cannot be null.");

            if (userIds.Count == 0)
                return new List<StudentProfileResponse>();

            if (userIds.Count > 100)
                throw new ValidationException("Maximum 100 user IDs per request.");

            var students = await _repo.GetByUserIdsAsync(userIds);
            return students.Select(Map).ToList();
        }

        public async Task<ProfileCompletionResponse> GetProfileCompletionStatusAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Profile not found for user {userId}.");

            var academicInfoCompleted = AcademicInfoComplete(student);
            var skillsCompleted = SkillsCompleted(student);
            var contactCompleted = ContactCompleted(student);
            var resumeUploaded = ResumeUploaded(student);

            return new ProfileCompletionResponse
            (
                academicInfoCompleted,
                skillsCompleted,
                contactCompleted,
                resumeUploaded,
                student.ProfileProgress
            );
        }

        private static StudentProfileShortResponse MapShortDto(Student student)
        {

            var academicInfoCompleted = AcademicInfoComplete(student);
            var skillsCompleted = SkillsCompleted(student);
            var contactCompleted = ContactCompleted(student);
            var resumeUploaded = ResumeUploaded(student);

            return new StudentProfileShortResponse
            (
                student.Id,
                student.FullName,
                student.Email,
                student.ProfileProgress,
                student.IsPlaced,
                academicInfoCompleted,
                skillsCompleted,
                contactCompleted,
                resumeUploaded
            );
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

        private static StudentProfileResponse Map(Student student)
        {
            return new StudentProfileResponse
            (
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