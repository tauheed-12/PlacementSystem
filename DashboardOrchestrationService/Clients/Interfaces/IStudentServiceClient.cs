// Clients/Interfaces/IStudentServiceClient.cs
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Interfaces
{
    public interface IStudentServiceClient
    {
        Task<StudentProfileDto> GetStudentProfileAsync(Guid studentId);
    }
}