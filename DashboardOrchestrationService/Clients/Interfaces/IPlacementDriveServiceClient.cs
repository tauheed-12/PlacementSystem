// Clients/Interfaces/IPlacementDriveServiceClient.cs
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Interfaces
{
    public interface IPlacementDriveServiceClient
    {
        Task<List<DriveDetailsDto>> GetDrivesByIds(List<Guid> driveIds);
        Task<int> GetTotalEligibleDrivesCount(Guid studentId); // added
    }
}