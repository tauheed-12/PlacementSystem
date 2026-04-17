using StudentService.Entities;

namespace StudentService.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken);
        Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task AddAsync(Student student, CancellationToken cancellationToken);
        Task DeleteAsync(Student student, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
        Task<List<Student>> GetAllAsync(CancellationToken cancellationToken);
        Task<List<Student>> GetByUserIdsAsync(List<Guid> userIds, CancellationToken cancellationToken);
    }
}
