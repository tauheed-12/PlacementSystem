namespace NotificationService.Infrastructure.Email
{
    public interface IEmailProvider
    {
        Task SendAsync(string to, string subject, string body);
    }
}
