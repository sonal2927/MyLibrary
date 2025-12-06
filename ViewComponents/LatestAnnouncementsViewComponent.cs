using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraryManagementSystem.ViewComponents
{
    public class LatestAnnouncementsViewComponent : ViewComponent
    {
        private readonly LibraryDbContext _context;

        public LatestAnnouncementsViewComponent(LibraryDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var latestAnnouncements = _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            return View(latestAnnouncements);
        }
    }
}
