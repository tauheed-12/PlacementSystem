using System.Net;
using System.Net.Http.Json;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Implementations;

public class StudentServiceClient : IStudentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StudentServiceClient> _logger;

    public StudentServiceClient(HttpClient httpClient, ILogger<StudentServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<StudentProfileDto> GetStudentProfileAsync(Guid studentId)
    {
        var response = await _httpClient.GetAsync($"/api/student/{studentId}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<StudentProfileDto>();
            return result ?? throw new Exception("Student response was null");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Student {studentId} not found");
        }

        _logger.LogError("Student service failed with status {StatusCode}", response.StatusCode);

        throw new HttpRequestException(
            $"Student service error for {studentId}: {response.StatusCode}");
    }
}