using System.Collections.Generic;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class LibrarianReportViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalIssued { get; set; }
        public int TotalSubmitted { get; set; }

        // ✅ Newly Added for Dashboard
        public int PendingRequests { get; set; }
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalLibrarians { get; set; }

        public List<MostIssuedBookViewModel> MostIssuedBooks { get; set; } = new();
    }

    public class MostIssuedBookViewModel
    {
        public string BookName { get; set; } = string.Empty;
        public int IssueCount { get; set; }
    }
}
