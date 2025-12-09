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

            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"] ?? "587");
            var user = smtpSection["Username"];

            // 🚀 READ PASSWORD FROM RAILWAY ENV VARIABLE FIRST
            var pass = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                       ?? smtpSection["Password"]; // fallback for local use

            var from = smtpSection["From"];
            var enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");

            if (string.IsNullOrWhiteSpace(from))
                throw new InvalidOperationException("SMTP 'From' address is not configured.");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass)
            };

           var mail = new MailMessage()
            {
                From = new MailAddress(from, "MyLibrary System"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mail.To.Add(toEmail);


            await client.SendMailAsync(mail);
        }
    }
}
