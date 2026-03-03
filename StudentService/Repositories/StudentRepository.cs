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

        public async Task<Student?> GetByUserIdAsync(Guid userId)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Student?> GetByIdAsync(Guid studentId)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<bool> ExistsByUserIdAsync(Guid userId)
        {
            return await _db.Students.AnyAsync(s => s.UserId == userId);
        }

        public async Task AddAsync(Student student)
        {
            await _db.Students.AddAsync(student);
        }

        public async Task DeleteAsync(Student student)
        {
            _db.Students.Remove(student);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<List<Student>> GetAllAsync()
        {
            return await _db.Students
                .Include(s => s.Skills)
                .ToListAsync();
        }

        public async Task<List<Student>> GetByUserIdsAsync(List<Guid> userIds)
        {
            return await _db.Students
                .Include(s => s.Skills)
                .Where(s => userIds.Contains(s.UserId))
                .ToListAsync();
        }
    }
}
