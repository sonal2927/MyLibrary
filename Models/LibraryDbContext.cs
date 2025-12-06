using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Models
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookRecord> BookRecords { get; set; }
        public DbSet<BookRequest> BookRequests { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Make LoginId optional
    modelBuilder.Entity<User>()
        .Property(u => u.LoginId)
        .IsRequired(false);

    // Unique index for LoginId (still optional)
    modelBuilder.Entity<User>()
        .HasIndex(u => u.LoginId)
        .IsUnique();

    // BookRecord relationship with optional LoginId
    modelBuilder.Entity<BookRecord>()
        .HasOne(br => br.User)
        .WithMany()
        .HasForeignKey(br => br.UserId)
       
        .IsRequired(false) // ✅ Make relationship optional
        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
}


        public DbSet<RenewalRequest> RenewalRequests { get; set; }

    }
}
