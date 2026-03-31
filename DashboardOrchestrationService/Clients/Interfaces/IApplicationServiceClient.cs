// Clients/Interfaces/IApplicationServiceClient.cs
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Interfaces
{
    public interface IApplicationServiceClient
    {
        Task<List<ApplicationStatusResponse>> GetApplicationsByUserId(Guid userId);
    }
}