using ApplicationService.DTO;
using static ApplicationService.DTO.Dtos;

namespace ApplicationService.Services.Interfaces
{
    public interface IApplicationService
    {
        // ---------------- COMMANDS ----------------

        Task ApplyAsync(CreateApplicationRequest request, CancellationToken cancellationToken);

        Task DeleteApplicationAsync(Guid applicationId, Guid studentId, CancellationToken cancellationToken);

        // ---------------- QUERIES ----------------

        Task<List<UserApplicationSummary>> GetUserApplicationsAsync(Guid studentId, CancellationToken cancellationToken);

        Task<List<ApplicationResponse>> GetDriveApplicationsAsync(Guid driveId, CancellationToken cancellationToken);

        Task<List<StudentApplication>> GetStudentApplicationsAsync(Guid studentId, CancellationToken cancellationToken);
    }
}