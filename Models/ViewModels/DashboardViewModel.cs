using System.Collections.Generic;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public User? User { get; set; }  // Logged-in student
        public List<BookRecord>? BookRequests { get; set; }  // Issued books
        public List<Book>? FeaturedBooks { get; set; }  // Explore section
        public List<string>? Notifications { get; set; }  // Alerts

        // 📢 Announcements
        public List<Announcement> Announcements { get; set; } = new();
    }
}
