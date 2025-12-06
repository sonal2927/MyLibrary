namespace LibraryManagementSystem.Models.ViewModels
{
    public class IssuedBookReportViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        public DateTime? IssuedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        // ✅ NEW formatted properties for Razor
        public string IssuedAtStr => IssuedAt?.ToString("dd MMM yyyy") ?? "-";
        public string ReturnedAtStr => ReturnedAt?.ToString("dd MMM yyyy") ?? "-";
    }
}
