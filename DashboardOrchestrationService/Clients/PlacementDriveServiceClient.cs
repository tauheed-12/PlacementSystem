using System.Net;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Implementations;

public class PlacementDriveServiceClient : IPlacementDriveServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlacementDriveServiceClient> _logger;

    public PlacementDriveServiceClient(HttpClient httpClient, ILogger<PlacementDriveServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<DriveDetailsDto>> GetDrivesByIdsAsync(List<Guid> driveIds)
    {
        if (driveIds == null || driveIds.Count == 0)
            return [];

        var response = await _httpClient.PostAsJsonAsync("/api/placementdrives/by-ids", driveIds);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<DriveDetailsDto>>();
            return result ?? [];
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        _logger.LogError("Drive service failed fetching drives. Status: {StatusCode}", response.StatusCode);

        throw new HttpRequestException($"Drive service error: {response.StatusCode}");
    }

    public async Task<int> GetEligibleDrivesCountAsync(Guid studentId)
    {
        var response = await _httpClient.GetAsync($"/api/placementdrives/eligible-count/{studentId}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;
        }

        if (response.StatusCode == HttpStatusCode.NotFound) return 0;

        _logger.LogError("Drive count failed for {StudentId} with {StatusCode}",
            studentId, response.StatusCode);

        throw new HttpRequestException($"Drive count service error: {response.StatusCode}");
    }
}