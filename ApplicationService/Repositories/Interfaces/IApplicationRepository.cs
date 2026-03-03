using ApplicationService.Entities;

namespace ApplicationService.Repositories.Interfaces
{
    public interface IApplicationRepository
    {
        Task AddAsync(Application application, CancellationToken cancellationToken);

        void Remove(Application application);

        Task<List<Application>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken);

        Task<List<Application>> GetByDriveIdAsync(Guid driveId,  CancellationToken cancellationToken);
        Task<Application?> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken);
    }
}
