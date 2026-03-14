namespace StudentService.Services.Interfaces
{
    public interface ISkillService
    {
        Task AddSkillAsync(Guid userId, string skillName);
        Task RemoveSkillAsync(Guid userId, Guid skillId);
        Task<List<string>> GetSkillsAsync(Guid userId);
    }
}
