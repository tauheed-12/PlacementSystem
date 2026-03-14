using NotificationsService.Clients.Interfaces;

namespace NotificationsService.Clients
{
    public class StudentServiceClient : IStudentServiceClient
    {
        private readonly HttpClient _httpClient;
        public StudentServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string?> GetEmailByUserId(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/students/{userId}/email");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
        public async Task<List<string>?> GetBulkEmailIds(List<Guid> userIds)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/students/bulk-emails", userIds);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
            }
            return null;
        }
    }
}
