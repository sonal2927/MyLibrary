using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public enum BookStatus
    {
        Pending,           
        Issued,            
        Submitted,         
        Cancelled,         
        ReturnRequested,   
        RenewalRequested,  
        Rejected,          
        Requested          
    }

    public class BookRecord
    {
        [Key]
        public int Id { get; set; }

        // 🔗 Foreign Keys
        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int UserId { get; set; }     // FK (int matches User.Id)
        public User User { get; set; } = null!;

        
        
        public string LoginId { get; set; } = string.Empty; // Store LoginId for reference

        // 🕒 Timestamps
        public DateTime? RequestedAt { get; set; }
        public DateTime? IssuedAt { get; set; }     
        public DateTime? DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }  

        // Duration info
        public int? RequestedDays { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Status
        public BookStatus Status { get; set; }

        // ⭐ Rating
        public int? Rating { get; set; }

        // Renewal support
        public DateTime? RenewedAt { get; set; }

        // Notification flag
        public bool Notified { get; set; } = false;
    }
}
