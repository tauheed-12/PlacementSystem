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
    }

}
