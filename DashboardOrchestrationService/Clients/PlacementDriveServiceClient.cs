// Clients/PlacementDriveServiceClient.cs
using System.Net.Http.Json;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients
{
    public class PlacementDriveServiceClient : IPlacementDriveServiceClient
    {
        private readonly HttpClient _httpClient;

        public PlacementDriveServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DriveDetailsDto>> GetDrivesByIds(List<Guid> driveIds)
        {
            if (driveIds.Count == 0)
                return [];

            // POST because query strings can't handle large lists of GUIDs cleanly
            var response = await _httpClient.PostAsJsonAsync("/api/placementdrives/by-ids", driveIds);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DriveDetailsDto>>();
                return result ?? [];
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return [];

            throw new HttpRequestException(
                $"PlacementDriveService failed fetching drive details. Status: {response.StatusCode}");
        }

        public async Task<int> GetTotalEligibleDrivesCount(Guid studentId)
        {
            //var response = await _httpClient.GetAsync($"/api/placementdrives/eligible-count/{studentId}");

            //if (response.IsSuccessStatusCode)
            //{
            //    var result = await response.Content.ReadFromJsonAsync<int>();
            //    return result;
            //}

            //if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            //    return 0;

            //throw new HttpRequestException(
            //    $"PlacementDriveService failed fetching eligible count. Status: {response.StatusCode}");
            return 30;
        }
    }
}