using StudentService.Entities;
using StudentService.Exceptions;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;

namespace StudentService.Services
{
    public class SkillService : ISkillService
    {
        private readonly IStudentRepository _repo;

        public SkillService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public async Task AddSkillAsync(Guid userId, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
                throw new ValidationException("Skill name cannot be empty");

            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            if (student.Skills.Any(s =>
                s.SkillName.ToLower() == skillName.ToLower()))
                throw new ConflictException("Skill already exists");

            student.Skills.Add(new StudentSkill
            {
                Id = Guid.NewGuid(),
                SkillName = skillName
            });

            await _repo.SaveChangesAsync();
        }

        public async Task RemoveSkillAsync(Guid userId, Guid skillId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            var skill = student.Skills.FirstOrDefault(s => s.Id == skillId);

            if (skill == null)
                throw new NotFoundException("Skill not found");

            student.Skills.Remove(skill);

            await _repo.SaveChangesAsync();
        }

        public async Task<List<string>> GetSkillsAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            return student.Skills.Select(s => s.SkillName).ToList();
        }
    }
}