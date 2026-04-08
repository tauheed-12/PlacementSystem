using PlacementDriveService.DTOs;

namespace PlacementDriveService.Services.Interfaces
{
    public interface IPlacementDriveService
    {
        Task<Guid> CreateDriveAsync(DriveCreateRequest request, Guid userId);
        Task UpdateDriveAsync(Guid id, DriveUpdateRequest request);
        Task DeleteDriveAsync(Guid id);
        Task<List<DriveResponse>> GetOpenDrivesAsync(int page, int pageSize);
        Task<DriveResponse> GetDriveByIdAsync(Guid id);
        Task<List<DriveResponse>> GetDrivesBulkAsync(List<Guid> driveIds);
    }
}
