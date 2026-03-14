namespace NotificationsService.Clients.Interfaces
{
    public interface IStudentServiceClient
    {
        Task<string?> GetEmailByUserId(Guid userId);
        Task<List<string>?> GetBulkEmailIds(List<Guid> userIds);
    }
}
