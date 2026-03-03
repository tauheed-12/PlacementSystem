using PlacementDriveService.DTOs;

namespace PlacementDriveService.Services.Interfaces
{
    public interface IPlacementDriveService
    {
        Task<Guid> CreateDriveAsync(PlacementDriveCreateDto dto);
        Task UpdateDriveAsync(Guid id, PlacementDriveUpdateDto dto);
        Task DeleteDriveAsync(Guid id);
        Task<List<PlacementDriveResponseDto>> GetOpenDrivesAsync(int page, int pageSize);
        Task<PlacementDriveResponseDto> GetDriveByIdAsync(Guid id);
        Task ApplyAsync(Guid driveId, Guid studentId);
        Task WithdrawAsync(Guid driveId, Guid studentId);
        Task<List<PlacementApplicationResponseDto>> GetStudentApplications(Guid studentId);
    }

}
