using ApplicationService.DTO;
using ApplicationService.HttpClients.Interfaces;
using static ApplicationService.DTO.Dtos;

namespace ApplicationService.HttpClients
{
    public class PlacementDriveServiceClient : IPlacementDriveServiceClient
    {
        private readonly HttpClient _httpClient;
        public PlacementDriveServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<Guid, PlacementDriveDetails>> GetDrivesBulkAsync(List<Guid> driveIds, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/drives/bulk", driveIds, cancellationToken);
            response.EnsureSuccessStatusCode();

            var drives = await response.Content.ReadFromJsonAsync<List<PlacementDriveDetails>>();
            return drives!.ToDictionary(d => d.Id);
        }
    }
}
