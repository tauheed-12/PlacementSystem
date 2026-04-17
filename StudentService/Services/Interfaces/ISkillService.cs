using static StudentService.DTOs.Dtos;

namespace StudentService.Services.Interfaces
{
    public interface ISkillService
    {
        Task AddSkillAsync(Guid userId, AddSkillRequest skillName, CancellationToken cancellationToken);
        Task RemoveSkillAsync(Guid userId, Guid skillId, CancellationToken cancellationToken);
        Task<List<string>> GetSkillsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
