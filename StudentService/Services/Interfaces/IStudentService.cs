using Microsoft.AspNetCore.Mvc;
using StudentService.DTOs;

namespace StudentService.Services.Interfaces
{
    public interface IStudentService
    {
        Task CreateProfileAsync(Guid userId, CreateStudentProfileDto dto);
        Task<StudentProfileResponseDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateStudentProfileDto dto);
        Task DeleteProfileAsync(Guid studentId);
        Task<List<StudentProfileResponseDto>> GetAllProfilesAsync();
        Task<List<StudentProfileResponseDto>> GetProfilesInBulkAsync(List<Guid> userIds);

    }
}
