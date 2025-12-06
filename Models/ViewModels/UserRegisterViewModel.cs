using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class UserRegisterViewModel
    {
        internal string? LoginId;

        [Required]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public string Gender { get; set; } = null!;

        [Required]
        public string Department { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;

        // Student-specific fields
        public string? Semester { get; set; }
        public string? Year { get; set; }
        public string? EnrollmentNumber { get; set; }

        // Optional fields
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        // Main unified Login Identifier
        [Required]
        [Display(Name = "User ID / Enrollment Number")]
        public string UserIdentifier { get; set; } = null!;
    }
}
