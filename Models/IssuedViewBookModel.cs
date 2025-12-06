namespace LibraryManagementSystem.Models.ViewModels
{
    public class IssuedBookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime? DueAt { get; set; }
        public bool IsOverdue { get; set; }
    }
}
