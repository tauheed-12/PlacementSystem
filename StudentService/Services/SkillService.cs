using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StudentService.Entities;
using StudentService.Exceptions;
using StudentService.Repositories.Interfaces;
using StudentService.Services.Interfaces;

namespace StudentService.Services
{
    public class SkillService : ISkillService
    {
        private readonly IStudentRepository _repo;
        private readonly ILogger<SkillService> _logger;

        public SkillService(IStudentRepository repo, ILogger<SkillService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task AddSkillAsync(Guid userId, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
            {
                _logger.LogError("AddSkill validation failed for user {UserId}: skill name is empty", userId);
                throw new ValidationException("Skill name cannot be empty");
            }

            skillName = skillName.Trim();

            if (skillName.Length > 100)
            {
                _logger.LogError("AddSkill validation failed for user {UserId}: skill name too long", userId);
                throw new ValidationException("Skill name is too long (maximum 100 characters)");
            }

            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            if (student.Skills.Any(s => string.Equals(s.SkillName, skillName, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Attempt to add duplicate skill '{Skill}' for user {UserId}", skillName, userId);
                throw new ConflictException("Skill already exists");
            }

            const int MaxSkills = 100;
            if (student.Skills.Count >= MaxSkills)
            {
                _logger.LogWarning("User {UserId} has reached max skills ({MaxSkills})", userId, MaxSkills);
                throw new ValidationException($"Maximum number of skills ({MaxSkills}) reached");
            }

            student.Skills.Add(new StudentSkill
            {
                Id = Guid.NewGuid(),
                SkillName = skillName
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Skill '{Skill}' added for user {UserId}", skillName, userId);
        }

        public async Task RemoveSkillAsync(Guid userId, Guid skillId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            var skill = student.Skills.FirstOrDefault(s => s.Id == skillId);

            if (skill == null)
            {
                _logger.LogWarning("Skill {SkillId} not found for user {UserId}", skillId, userId);
                throw new NotFoundException("Skill not found");
            }

            student.Skills.Remove(skill);

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Skill {SkillId} removed for user {UserId}", skillId, userId);
        }

        public async Task<List<string>> GetSkillsAsync(Guid userId)
        {
            var student = await _repo.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Student profile not found");

            return student.Skills.Select(s => s.SkillName).ToList();
        }
    }
}