using ApplicationService.Data;
using ApplicationService.Entities;
using ApplicationService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Application application, CancellationToken cancellationToken)
        {
            await _context.Applications.AddAsync(application, cancellationToken);
        }

        public void Remove(Application application)
        {
            _context.Applications.Remove(application);
        }

        public async Task<List<Application>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken)
        {
            return await _context.Applications
                .Where(app => app.StudentUserId == studentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Application>> GetByDriveIdAsync(Guid driveId, CancellationToken cancellationToken)
        {
            return await _context.Applications
                .Where(app => app.DriveId == driveId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Application?> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken)
        {
            return await _context.Applications.FindAsync(new object[] {applicationId}, cancellationToken);
        }
    }
}
