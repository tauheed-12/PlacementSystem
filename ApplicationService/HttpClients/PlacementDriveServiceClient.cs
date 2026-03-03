using ApplicationService.DTO;
using ApplicationService.HttpClients.Interfaces;

namespace ApplicationService.HttpClients
{
    public class PlacementDriveServiceClient : IPlacementDriveServiceClient
    {
        private readonly HttpClient _httpClient;
        public PlacementDriveServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<Guid, PlacementDriveDetailsDto>> GetDrivesBulkAsync(List<Guid> driveIds, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/drives/bulk", driveIds, cancellationToken);
            response.EnsureSuccessStatusCode();

            var drives = await response.Content.ReadFromJsonAsync<List<PlacementDriveDetailsDto>>();
            return drives!.ToDictionary(d => d.Id);
        }
    }
}
