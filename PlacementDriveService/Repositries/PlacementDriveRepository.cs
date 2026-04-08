using Microsoft.EntityFrameworkCore;
using PlacementDriveService.Data;
using PlacementDriveService.Entities;
using PlacementDriveService.Enums;
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

        public async Task AddOutboxMessageAsync(OutboxMessage message)
        {
            await _db.OutboxMessages.AddAsync(message);
        }

        public async Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50)
        {
            return await _db.OutboxMessages
                .Where(msg => !msg.IsProcessed)
                .OrderBy(msg => msg.CreatedAt)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<PlacementDrive> GetOpenDrives()
        {
            return _db.PlacementDrives
                .Where(d => d.Status == DriveStatus.Scheduled &&
                            d.ApplicationDeadline >= DateTime.UtcNow);
        }
    }

}
