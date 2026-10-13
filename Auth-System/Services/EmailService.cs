using Auth_System.IService;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace Auth_System.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var from = _config["Email:From"]
                ?? throw new InvalidOperationException("Email:From is missing.");
            var host = _config["Email:Host"]
                ?? throw new InvalidOperationException("Email:Host is missing.");
            var port = int.Parse(_config["Email:Port"] ?? "587");
            var username = _config["Email:Username"]
                ?? throw new InvalidOperationException("Email:Username is missing.");
            var password = _config["Email:Password"]
                ?? throw new InvalidOperationException("Email:Password is missing.");
            var displayName = _config["Email:DisplayName"] ?? "Auth System";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(displayName, from));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

    }
}
