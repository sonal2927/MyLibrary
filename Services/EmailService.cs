using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

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

            var host = smtpSection["Host"] ?? "smtp-relay.sendinblue.com";
            var port = int.Parse(smtpSection["Port"] ?? "465"); // Use 465 SSL by default
            var user = smtpSection["Username"];
            var pass = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? smtpSection["Password"];
            var from = smtpSection["From"];

            if (string.IsNullOrWhiteSpace(from))
                throw new InvalidOperationException("SMTP 'From' address is not configured.");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,                  // SSL for 465 or STARTTLS for 587/2525
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(user, pass),
                Timeout = 15000                     // 15 seconds timeout
            };

            var mail = new MailMessage()
            {
                From = new MailAddress(from, "MyLibrary System"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mail.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mail);
                Console.WriteLine($"✅ Email sent to {toEmail}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"❌ SMTP Exception: {ex.StatusCode} - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Exception: {ex.Message}");
                throw;
            }
        }
    }
}
