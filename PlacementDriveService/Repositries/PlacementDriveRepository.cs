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

        public async Task AddAsync(PlacementDrive drive, CancellationToken ct)
        {
            await _db.PlacementDrives.AddAsync(drive, ct);
        }

        public async Task<PlacementDrive?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _db.PlacementDrives.FindAsync(id, ct);
        }

        public async Task DeleteAsync(PlacementDrive drive, CancellationToken ct)
        {
            _db.PlacementDrives.Remove(drive);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }

        public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken ct)
        {
            await _db.OutboxMessages.AddAsync(message, ct);
        }

        public async Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50, CancellationToken ct = default)
        {
            return await _db.OutboxMessages
                .Where(msg => !msg.IsProcessed)
                .OrderBy(msg => msg.CreatedAt)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public IQueryable<PlacementDrive> GetOpenDrives(CancellationToken ct)
        {
            return _db.PlacementDrives
                .Where(d => d.Status == DriveStatus.Scheduled &&
                            d.ApplicationDeadline >= DateTime.UtcNow);
        }
    }

}
