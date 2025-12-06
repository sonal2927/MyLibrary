using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class RenewalRequest
    {
        public int Id { get; set; }

        public int BookRecordId { get; set; }

        public BookRecord BookRecord { get; set; } = null!;

        [Required]
        [Range(1, 30)]
        public int RequestedDays { get; set; }

        public string LoginId { get; set; } = string.Empty;
 
        public int UserId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public bool? IsApproved { get; set; }  // null = pending, true/false = decision

        public DateTime? ApprovedAt { get; set; }

        public string? ReviewedBy { get; set; }
    }
}
