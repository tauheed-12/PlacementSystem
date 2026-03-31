// Clients/StudentServiceClient.cs
using System.Net.Http.Json;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients
{
    public class StudentServiceClient : IStudentServiceClient
    {
        private readonly HttpClient _httpClient;

        public StudentServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<StudentProfileDto> GetStudentProfileById(Guid studentId)
        {
            var response = await _httpClient.GetAsync($"/api/student/{studentId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<StudentProfileDto>();

                return result ?? throw new KeyNotFoundException(
                    $"Student profile returned empty for ID {studentId}");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"Student {studentId} not found");

            throw new HttpRequestException(
                $"StudentService failed for student {studentId}. Status: {response.StatusCode}");
        }
    }
}