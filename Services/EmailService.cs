using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LibraryManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
{
    var smtpSection = _config.GetSection("Smtp");
    var host = smtpSection["Host"];
    var port = int.Parse(smtpSection["Port"] ?? "587");
    var user = smtpSection["Username"];
    var pass = smtpSection["Password"];
    var from = smtpSection["From"];
    var enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");

    if (string.IsNullOrWhiteSpace(from))
        throw new InvalidOperationException("SMTP 'From' address is not configured.");

    using var client = new SmtpClient(host, port)
    {
        EnableSsl = enableSsl,
        Credentials = new NetworkCredential(user, pass)
    };

    var mail = new MailMessage(from, toEmail, subject, htmlMessage)
    {
        IsBodyHtml = true
    };

    await client.SendMailAsync(mail);
}

    }
}
