namespace NotificationsService.Clients.Interfaces
{
    public interface IEmailClient
    {
        Task SendAsync(string to, string subject, string body);
    }
}
