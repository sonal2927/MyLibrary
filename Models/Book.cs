using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        public string Department { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public string SerializedImages { get; set; } = "[]";

        public string? ISBN { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
        public decimal Price { get; set; } = 0;

        [Required(ErrorMessage = "Book code is required.")]
        public string BookCode { get; set; } = string.Empty;

        // Navigation property to BookRecords (for issue/return tracking)
        public ICollection<BookRecord>? BookRecords { get; set; }

        [NotMapped]
        public List<string> ImageList
        {
            get
            {
                try
                {
                    return string.IsNullOrWhiteSpace(SerializedImages)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(SerializedImages) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                SerializedImages = JsonSerializer.Serialize(value ?? new List<string>());
            }
        }

        [NotMapped]
        public string ImagePath =>
            ImageList.FirstOrDefault() ?? "/images/books/default-book.png";

        public string Status => IsAvailable ? "Available" : "Issued";


    }
}
