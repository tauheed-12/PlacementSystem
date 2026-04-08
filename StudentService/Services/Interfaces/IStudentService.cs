using Microsoft.AspNetCore.Mvc;
using StudentService.DTOs;
using StudentService.Entities;

namespace StudentService.Services.Interfaces
{
    public interface IStudentService
    {
        Task CreateProfileAsync(Guid userId, Dtos.CreateStudentProfileRequest request);
        Task<Dtos.StudentProfileResponse> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, Dtos.UpdateStudentProfileRequest dto);
        Task DeleteProfileAsync(Guid studentId);
        Task<List<Dtos.StudentProfileResponse>> GetAllProfilesAsync();
        Task<List<Dtos.StudentProfileResponse>> GetProfilesInBulkAsync(List<Guid> userIds);
        Task<Dtos.ProfileCompletionResponse> GetProfileCompletionStatusAsync(Guid userId);
        Task<Dtos.StudentProfileShortResponse> GetProfileByIdAsync(Guid studentId);
    }
}
