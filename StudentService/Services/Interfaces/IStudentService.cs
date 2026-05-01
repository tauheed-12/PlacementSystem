using StudentService.DTOs;

namespace StudentService.Services.Interfaces
{
    public interface IStudentService
    {
        Task CreateProfileAsync(Guid userId, string Email, Dtos.CreateStudentProfileRequest request, CancellationToken cancellationToken);
        Task<Dtos.StudentProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
        Task UpdateProfileAsync(Guid userId, Dtos.UpdateStudentProfileRequest dto, CancellationToken cancellationToken);
        Task DeleteProfileAsync(Guid userId, CancellationToken cancellationToken);
        Task<List<Dtos.StudentProfileResponse>> GetAllProfilesAsync(CancellationToken cancellationToken);
        Task<List<Dtos.StudentProfileResponse>> GetProfilesInBulkAsync(List<Guid> userIds, CancellationToken cancellationToken);
        Task<Dtos.ProfileCompletionResponse> GetProfileCompletionStatusAsync(Guid userId, CancellationToken cancellationToken);
        Task<Dtos.StudentProfileShortResponse> GetProfileByIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
