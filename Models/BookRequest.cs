using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BookRequest
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string? Publisher { get; set; }

        public int? PublishedYear { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [Required]
        public string StudentId { get; set; } = string.Empty; // From Session or User.Identity.Name

        public string? LoginId { get; set; }

        public User? User { get; set; }

    }
}
