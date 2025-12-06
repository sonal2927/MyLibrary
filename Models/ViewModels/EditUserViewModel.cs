using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; } // Database PK (int)

        [MaxLength(50)]
        public string? LoginId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Readonly in view

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        // ✅ Password is optional during edit
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
