using PlacementDriveService.Entities;

namespace PlacementDriveService.Repositries.Interfaces
{
    public interface IPlacementDriveRepository
    {
        Task AddAsync(PlacementDrive drive, CancellationToken ct);
        Task<PlacementDrive?> GetByIdAsync(Guid id, CancellationToken ct);
        Task DeleteAsync(PlacementDrive drivem, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        IQueryable<PlacementDrive> GetOpenDrives(CancellationToken ct);
        Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken ct);
        Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50, CancellationToken ct = default);
    }
}
