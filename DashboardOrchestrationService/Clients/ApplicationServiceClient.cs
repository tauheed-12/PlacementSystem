// Clients/ApplicationServiceClient.cs
using System.Net.Http.Json;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients
{
    public class ApplicationServiceClient : IApplicationServiceClient
    {
        private readonly HttpClient _httpClient;

        public ApplicationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ApplicationStatusResponse>> GetApplicationsByUserId(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/applications/status/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ApplicationStatusResponse>>();
                return result ?? [];
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return [];

            throw new HttpRequestException(
                $"ApplicationService failed for user {userId}. Status: {response.StatusCode}");
        }
    }
}