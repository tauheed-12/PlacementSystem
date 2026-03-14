using Microsoft.EntityFrameworkCore;
using PlacementDriveService.Constants;
using PlacementDriveService.Data;
using PlacementDriveService.Entities;
using PlacementDriveService.Repositries.Interfaces;

namespace PlacementDriveService.Repositries
{
    public class PlacementDriveRepository : IPlacementDriveRepository
    {
        private readonly PlacementDriveDbContext _db;

        public PlacementDriveRepository(PlacementDriveDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(PlacementDrive drive)
        {
            await _db.PlacementDrives.AddAsync(drive);
        }

        public async Task<PlacementDrive?> GetByIdAsync(Guid id)
        {
            return await _db.PlacementDrives.FindAsync(id);
        }

        public async Task DeleteAsync(PlacementDrive drive)
        {
            _db.PlacementDrives.Remove(drive);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public IQueryable<PlacementDrive> GetOpenDrives()
        {
            return _db.PlacementDrives
                .Where(d => d.Status == DriveStatus.Scheduled &&
                            d.ApplicationDeadline >= DateTime.UtcNow);
        }

        public async Task<bool> HasStudentApplied(Guid driveId, Guid studentId)
        {
            return await _db.PlacementApplications
                .AnyAsync(a => a.PlacementDriveId == driveId &&
                               a.StudentUserId == studentId);
        }

        public async Task AddApplicationAsync(PlacementApplication application)
        {
            await _db.PlacementApplications.AddAsync(application);
        }

        public void RemoveApplication(PlacementApplication application)
        {
            _db.PlacementApplications.Remove(application);
        }

        public async Task<PlacementApplication?> GetApplication(Guid driveId, Guid studentId)
        {
            return await _db.PlacementApplications
                .FirstOrDefaultAsync(a => a.PlacementDriveId == driveId &&
                                          a.StudentUserId == studentId);
        }

        public IQueryable<PlacementApplication> GetStudentApplications(Guid studentId)
        {
            return _db.PlacementApplications
                .Include(a => a.PlacementDrive)
                .Where(a => a.StudentUserId == studentId);
        }
    }

}
