using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly LibraryDbContext _context;

        public AnnouncementController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Announcement model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string? createdBy = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(createdBy))
                return RedirectToAction("List", "Announcement");

            model.CreatedBy = createdBy;
            model.CreatedAt = DateTime.Now;

            _context.Announcements.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "✅ Announcement posted successfully.";
            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult List()
        {
            var list = _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return View(list);
        }
    }
}
