using PlacementDriveService.DTOs;

namespace PlacementDriveService.Services.Interfaces
{
    public interface IPlacementDriveService
    {
        Task<Guid> CreateDriveAsync(DriveCreateRequest request, Guid userId, CancellationToken ct);
        Task UpdateDriveAsync(Guid id, DriveUpdateRequest request, CancellationToken ct);
        Task DeleteDriveAsync(Guid id, CancellationToken ct);
        Task<List<DriveResponse>> GetOpenDrivesAsync(int page, int pageSize, CancellationToken ct);
        Task<DriveResponse> GetDriveByIdAsync(Guid id, CancellationToken ct);
        Task<List<DriveResponse>> GetDrivesBulkAsync(List<Guid> driveIds, CancellationToken ct);
    }
}
