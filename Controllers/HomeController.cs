using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Test()
        {
            return Content("Layout working ✅");
        }

        public IActionResult Index()
        {
            // Fetch the latest announcement from the database
            using var context = new Models.LibraryDbContext(
                new Microsoft.EntityFrameworkCore.DbContextOptions<Models.LibraryDbContext>());
            var latestAnnouncement = context.Announcements
                .OrderByDescending(a => a.CreatedAt);

            return View();
        }


        public IActionResult AboutLibrary()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
