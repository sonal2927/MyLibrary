using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LibraryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static void Seed(LibraryDbContext context)
        {
            // ⚠️ Temporarily comment this out to allow reseeding (then uncomment later)
            // if (context.Books.Any())
            //     return;

            var books = new List<Book>
            {
                new Book {
                    Title = "Introduction to Algorithms",
                    Author = "Thomas H. Cormen",
                    Department = "CSE",
                    IsAvailable = true,
                    SerializedImages = JsonSerializer.Serialize(new List<string> { "/images/books/algorithm.jpg" })
                },
                new Book {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Department = "IT",
                    IsAvailable = true,
                    SerializedImages = JsonSerializer.Serialize(new List<string> { "/images/books/clean_code.jpg" })
                },
                new Book {
                    Title = "Thermodynamics Basics",
                    Author = "Y.V.C. Rao",
                    Department = "Mechanical",
                    IsAvailable = true,
                    SerializedImages = JsonSerializer.Serialize(new List<string> { "/images/books/thermo.jpg" })
                },
                new Book {
                    Title = "Organic Chemistry",
                    Author = "Morrison & Boyd",
                    Department = "Chemical",
                    IsAvailable = true,
                    SerializedImages = JsonSerializer.Serialize(new List<string> { "/images/books/chemistry.jpg" })
                },
                new Book {
                    Title = "Structural Analysis",
                    Author = "R.C. Hibbeler",
                    Department = "Civil",
                    IsAvailable = true,
                    SerializedImages = JsonSerializer.Serialize(new List<string> { "/images/books/structure.jpg" })
                },
                new Book {
                    Title = "Basic Electrical Engineering",
                    Author = "D.P. Kothari",
                    Department = "Electrical",
                    IsAvailable = true,
                    SerializedImages = JsonSerializer.Serialize(new List<string> { "/images/books/electrical.jpg" })
                }
            };

            context.Books.AddRange(books);
            context.SaveChanges();
        }
    }
}
