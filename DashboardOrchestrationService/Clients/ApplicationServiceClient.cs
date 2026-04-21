using System.Net;
using System.Net.Http.Json;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Implementations;

public class ApplicationServiceClient : IApplicationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApplicationServiceClient> _logger;

    public ApplicationServiceClient(HttpClient httpClient, ILogger<ApplicationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ApplicationStatusDto>> GetApplicationsAsync(Guid studentId)
    {
        var response = await _httpClient.GetAsync($"/api/applications/status/{studentId}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<ApplicationStatusDto>>();
            return result ?? [];
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        _logger.LogError("Application service failed for {StudentId} with {StatusCode}",
            studentId, response.StatusCode);

        throw new HttpRequestException(
            $"Application service error: {response.StatusCode}");
    }
}