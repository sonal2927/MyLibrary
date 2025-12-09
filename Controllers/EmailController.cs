using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Services;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        // 🔍 TEST ROUTE
        // URL: /Email/Test
        public async Task<IActionResult> Test()
        {
            try
            {
                string to = "sonalgohil027@gmail.com"; // your email
                string subject = "📨 Test Email from MyLibrary";
                string body = "<h2>Hello Sonal!</h2><p>Your email system is working perfectly ✔</p>";

                await _emailService.SendEmailAsync(to, subject, body);

                return Content("✔ Test email sent! Check your inbox.");
            }
            catch (System.Exception ex)
            {
                return Content("❌ Error: " + ex.Message);
            }
        }
    }
}
