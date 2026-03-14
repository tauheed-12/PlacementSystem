using NotificationsService.Clients.Interfaces;
using System.Net;
using System.Net.Mail;

namespace NotificationsService.Clients
{
    public class EmailClient : IEmailClient
    {
        private readonly IConfiguration _configuration;

        public EmailClient(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? _configuration["Email:SmtpHost"];
            var smtpPortRaw = _configuration["EmailSettings:SmtpPort"] ?? _configuration["Email:Port"];
            var smtpUser = _configuration["EmailSettings:SmtpUser"] ?? _configuration["Email:Username"];
            var smtpPass = _configuration["EmailSettings:SmtpPass"] ?? _configuration["Email:Password"];
            var fromAddress = _configuration["EmailSettings:FromAddress"] ?? _configuration["Email:From"];

            if (string.IsNullOrWhiteSpace(smtpHost))
                throw new InvalidOperationException("SMTP host is not configured (EmailSettings:SmtpHost).");
            if (!int.TryParse(smtpPortRaw, out var smtpPort))
                throw new InvalidOperationException("SMTP port is not configured or invalid (EmailSettings:SmtpPort).");
            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
                throw new InvalidOperationException("SMTP credentials are missing (EmailSettings:SmtpUser / EmailSettings:SmtpPass).");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress ?? smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage).ConfigureAwait(false);
        }
    }
}




