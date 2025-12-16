using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            // 🔐 Read ONLY from Environment Variables
            var host = Environment.GetEnvironmentVariable("SMTP_HOST");
            var port = Environment.GetEnvironmentVariable("SMTP_PORT");
            var username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var from = Environment.GetEnvironmentVariable("SMTP_FROM");

            // 🛑 Safety check
            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from))
            {
                throw new Exception("SMTP environment variables are not fully configured.");
            }

            using var client = new SmtpClient(host, int.Parse(port))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var mail = new MailMessage(from, toEmail, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
