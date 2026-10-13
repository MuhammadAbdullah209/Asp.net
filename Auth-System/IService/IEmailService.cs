namespace Auth_System.IService
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
