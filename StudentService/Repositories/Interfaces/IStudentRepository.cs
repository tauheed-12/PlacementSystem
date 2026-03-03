using StudentService.Entities;

namespace StudentService.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByUserIdAsync(Guid userId);
        Task<Student?> GetByIdAsync(Guid studentId);
        Task<bool> ExistsByUserIdAsync(Guid userId);
        Task AddAsync(Student student);
        Task DeleteAsync(Student student);
        Task SaveChangesAsync();
        Task<List<Student>> GetAllAsync();
        Task<List<Student>> GetByUserIdsAsync(List<Guid> userIds);
    }
}
