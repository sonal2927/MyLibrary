using System.Collections.Generic;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class FacultyDashboardViewModel
    {
        // 👩‍🏫 Faculty profile details
        public User Faculty { get; set; } = new(); // ✅ Initialized to avoid CS8618 warning

        // 📚 Books currently issued to the faculty
        public List<BookRecord> IssuedBooks { get; set; } = new();

        // 🔍 Search results or recommended books by category
        public List<Book> CategoryWiseBooks { get; set; } = new();

        // 📦 Track status of book requests (approved/rejected/pending)
        public List<BookRecord> TrackedRequests { get; set; } = new(); // 🔄 Changed from BookRequest to BookRecord

        // 📢 General announcements from the library
        public List<string> LibraryNotices { get; set; } = new();

        // 📊 (Optional) Student reading activity (can remove if unused)
        public List<BookRecord> StudentReadingActivity { get; set; } = new();

         public List<Book> FeaturedBooks { get; set; } = new();
    }
}
