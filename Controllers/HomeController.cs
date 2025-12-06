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
