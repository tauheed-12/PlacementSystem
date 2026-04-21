namespace DashboardOrchestrationService.Clients.Interfaces;

using DashboardOrchestrationService.DTOs;

public interface IPlacementDriveServiceClient
{
    Task<List<DriveDetailsDto>> GetDrivesByIdsAsync(List<Guid> driveIds);
    Task<int> GetEligibleDrivesCountAsync(Guid studentId);
}