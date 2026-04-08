using PlacementDriveService.Entities;

namespace PlacementDriveService.Repositries.Interfaces
{
    public interface IPlacementDriveRepository
    {
        Task AddAsync(PlacementDrive drive);
        Task<PlacementDrive?> GetByIdAsync(Guid id);
        Task DeleteAsync(PlacementDrive drive);
        Task SaveChangesAsync();
        IQueryable<PlacementDrive> GetOpenDrives();
        Task AddOutboxMessageAsync(OutboxMessage message);
        Task<List<OutboxMessage>> GetUnProcessedOutboxMessagesAsync(int batchSize = 50);
    }
}
