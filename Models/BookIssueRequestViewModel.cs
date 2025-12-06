namespace LibraryManagementSystem.Models.ViewModels
{
    public class BookIssueRequestViewModel
    {
        public int Id { get; set; }              // From version 1
        public int BookId { get; set; }          // From version 2
        public string Title { get; set; } = "";  // Version 1
        public string BookTitle { get; set; } = ""; // Version 2

        public string Status { get; set; } = "";
        public DateTime RequestedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? DueAt { get; set; }

       public int LoginId { get; set; }
        public string? FullName { get; set; }
        public string? Department { get; set; }

        public string Author { get; set; } = string.Empty;

    }
}
