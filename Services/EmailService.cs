using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Services
{
    // ✅ INTERFACE (IMPORTANT)
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }

    // ✅ IMPLEMENTATION
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;

        public EmailService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Brevo API key is missing");

            _httpClient.DefaultRequestHeaders.Remove("api-key");
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

            var payload = new
            {
                sender = new
                {
                    name = "Library Management System",
                    email = "sonalgohil027@gmail.com"
                },
                to = new[]
                {
                    new { email = toEmail }
                },
                subject = subject,
                htmlContent = htmlMessage
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.brevo.com/v3/smtp/email",
                content
            );

            response.EnsureSuccessStatusCode();
        }
    }
}
