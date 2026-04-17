using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Entities;
using StudentService.Repositories.Interfaces;

namespace StudentService.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly StudentDbContext _db;

        public StudentRepository(StudentDbContext db)
        {
            _db = db;
        }

        public async Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        }

        public async Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _db.Students.AnyAsync(s => s.UserId == userId, cancellationToken);
        }

        public async Task AddAsync(Student student, CancellationToken cancellationToken)
        {
            await _db.Students.AddAsync(student, cancellationToken);
        }

        public async Task DeleteAsync(Student student, CancellationToken cancellationToken)
        {
            _db.Students.Remove(student);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Student>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Student>> GetByUserIdsAsync(List<Guid> userIds, CancellationToken cancellationToken)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .Where(s => userIds.Contains(s.UserId))
                .ToListAsync(cancellationToken);
        }
    }
}
