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
        Task<bool> HasStudentApplied(Guid driveId, Guid studentId);
        Task AddApplicationAsync(PlacementApplication application);
        void RemoveApplication(PlacementApplication application);
        Task<PlacementApplication?> GetApplication(Guid driveId, Guid studentId);
        IQueryable<PlacementApplication> GetStudentApplications(Guid studentId);
    }
}
