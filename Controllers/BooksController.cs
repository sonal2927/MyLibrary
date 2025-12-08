using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        private string? GetLoginId()
        {
            return HttpContext.Session.GetString("LoginId");
        }

        public IActionResult Index(string department = "All", string search = "")
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                books = books.Where(b => b.Title.Contains(search) || b.Author.Contains(search));

            if (!string.IsNullOrWhiteSpace(department) && department != "All")
                books = books.Where(b => b.Department == department);

            return View(books.ToList());
        }

        [HttpGet]
        public IActionResult IssueBookForm(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var libId = HttpContext.Session.GetString("LoginId");

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(libId))
            {
                TempData["Error"] = "Please login before issuing a book.";
                return RedirectToAction("Login", "Account");
            }

            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound();

            // ❌ NO NEED to manually load image list — it's handled by your Book model's getter

            return View("IssueBookForm", book);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IssueBook(int bookId)
        {
            var libId = HttpContext.Session.GetString("LoginId");

            if (string.IsNullOrEmpty(libId))
            {
                TempData["Error"] = "Login session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null || book.Quantity <= 0)
            {
                TempData["Error"] = "Book not available or doesn't exist.";
                return RedirectToAction("BrowseBooks");
            }

            TempData["Success"] = "✅ Your book issue request has been submitted. Please wait for librarian approval.";
            return RedirectToAction("BrowseBooks");
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            return book == null ? NotFound() : View(book);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile imageFile)
        {
            if (!ModelState.IsValid) return View(book);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/books/", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                book.ImageList = new List<string> { fileName };
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Book added successfully!";
            return RedirectToAction(nameof(BrowseBooks));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            return book == null ? NotFound() : View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();
            if (!ModelState.IsValid) return View(book);

            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Book updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.Id == book.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(BrowseBooks));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            return book == null ? NotFound() : View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "🗑️ Book deleted successfully!";
            }

            return RedirectToAction(nameof(BrowseBooks));
        }

        [HttpGet]
        public IActionResult Search(string title, string author, string category, string isbn)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                books = books.Where(b => b.Title.Contains(title));
            if (!string.IsNullOrEmpty(author))
                books = books.Where(b => b.Author.Contains(author));
            if (!string.IsNullOrEmpty(category))
                books = books.Where(b => b.Department.Contains(category));
            if (!string.IsNullOrEmpty(isbn))
                books = books.Where(b => !string.IsNullOrEmpty(b.ISBN) && b.ISBN.Contains(isbn));

            return View(books.ToList());
        }

        [HttpGet]
        public IActionResult IssuedBooks()
        {
            string? libId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(libId))
                return RedirectToAction("Login", "Account");

            var records = _context.BookRecords
                .Where(r => r.LoginId == libId && r.ReturnedAt == null)
                .Include(r => r.Book)
                .ToList();

            return View(records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestReturn(int recordId)
        {
            var record = _context.BookRecords.FirstOrDefault(r => r.Id == recordId);
            if (record != null && record.ReturnedAt == null)
            {
                record.Status = BookStatus.ReturnRequested;
                _context.SaveChanges();
                TempData["Success"] = "📩 Return request sent to librarian.";
            }
            return RedirectToAction("IssuedBooks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestRenewal(int recordId)
        {
            var record = _context.BookRecords.FirstOrDefault(r => r.Id == recordId);
            if (record != null && record.DueAt.HasValue && record.Status == BookStatus.Issued)
            {
                record.Status = BookStatus.RenewalRequested;
                _context.SaveChanges();
                TempData["Success"] = "🔁 Renewal request sent to librarian.";
            }
            return RedirectToAction("IssuedBooks");
        }

       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestBook(int id, int daysNeeded, DateTime fromDate, DateTime toDate)
        {
            // 🔹 1. UserId INT session se lo
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ToastMessage"] = "⚠️ Please login to request a book.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Login", "Account");
            }

            // 🔹 2. Book fetch karo
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                TempData["ToastMessage"] = "❌ Book not found.";
                TempData["ToastType"] = "danger";
                return RedirectToAction("StudentDashboard");
            }

            // 🔹 3. Current logged user fetch
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null)
            {
                TempData["ToastMessage"] = "❌ User not found.";
                TempData["ToastType"] = "danger";
                return RedirectToAction("Login", "Account");
            }

            // 🔹 4. New BookRecord banao (correct FKs)
            var record = new BookRecord
            {
                BookId = book.Id,
                UserId = user.Id,              // 💥 KEY POINT: Actual FK (INT)
                LoginId = user.LoginId ?? "",  // sirf reference ke liye, FK nahi

                RequestedAt = DateTime.Now,
                RequestedDays = daysNeeded,
                FromDate = fromDate,
                ToDate = toDate,
                Status = BookStatus.Requested
            };

            _context.BookRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "📘 Book request submitted!";
            TempData["ToastType"] = "success";

            // return redirect role-wise
            return user.Role.ToLower() switch
            {
                "student" => RedirectToAction("StudentDashboard"),
                "faculty" => RedirectToAction("FacultyDashboard"),
                "librarian" => RedirectToAction("LibrarianDashboard"),
                _ => RedirectToAction("Login", "Account")
            };
        }



         [HttpGet]
        public IActionResult StudentDashboard()
        {
            string? libId = HttpContext.Session.GetString("LoginId");

            // 🔐 If not logged in → redirect to login
            if (string.IsNullOrEmpty(libId))
                return RedirectToAction("Login", "Account");

            // 👩‍🎓 Get student details
            var student = _context.Users
                .FirstOrDefault(u => u.LoginId == libId && u.Role.ToLower() == "student");

            if (student == null)
                return RedirectToAction("Login", "Account");

            // 📘 Fetch ALL student requests (Requested + Issued + Submitted)
            var requests = _context.BookRecords
                .Where(r => r.LoginId == student.LoginId)
                .Include(r => r.Book)
                .OrderByDescending(r => r.IssuedAt)   // Latest on top
                .ToList();

            // 🌟 Featured books
            var featured = _context.Books
                .Where(b => b.IsAvailable)
                .OrderBy(b => Guid.NewGuid())
                .Take(6)
                .ToList();

            // 📢 Announcements
            var announcements = _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            // 🎯 Build ViewModel
            var model = new DashboardViewModel
            {
                User = student,

                // 🔥 IMPORTANT: your view expects all requests (not only issued)
                BookRequests = requests,

                FeaturedBooks = featured,

                Notifications = new List<string>
                {
                    "📘 New arrivals in your department!",
                    "⏳ Check your pending book requests!"
                },

                Announcements = announcements
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult FacultyDashboard()
        {
            string? libId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(libId))
                return RedirectToAction("Login", "Account");

            var faculty = _context.Users.FirstOrDefault(u => u.LoginId == libId && u.Role == "Faculty");
            if (faculty == null)
                return RedirectToAction("Login", "Account");

            var issued = _context.BookRecords
                .Where(r => r.LoginId == faculty.LoginId && r.IssuedAt != null && r.ReturnedAt == null)
                .Include(r => r.Book)
                .ToList();

            var featured = _context.Books
                .Where(b => b.IsAvailable)
                .OrderBy(b => Guid.NewGuid())
                .Take(6)
                .ToList();

            var announcements = _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            var model = new FacultyDashboardViewModel
            {
                Faculty = faculty,
                IssuedBooks = issued,
                FeaturedBooks = featured,
                LibraryNotices = announcements.Select(a => $"📢 {a.Title}").ToList()
            };

            return View(model);

        }

        

        [HttpGet]
        public IActionResult ManageBookRequests()
        {
            var requests = _context.BookRecords
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == BookStatus.Pending || r.Status == BookStatus.RenewalRequested || r.Status == BookStatus.ReturnRequested)
                .ToList();

            return View(requests);
        }

        // ✅ Paste here:

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveRequest(int id)
        {
            var record = _context.BookRecords.Include(r => r.Book).FirstOrDefault(r => r.Id == id);
            if (record == null) return NotFound();

            if (record.Status == BookStatus.Pending)
            {
                record.Status = BookStatus.Issued;
                record.IssuedAt = DateTime.Now;
                record.DueAt = DateTime.Now.AddDays(14);
                record.Book.Quantity -= 1;
            }
            else if (record.Status == BookStatus.RenewalRequested)
            {
                record.Status = BookStatus.Issued;
                record.DueAt = record.DueAt?.AddDays(7); // extend by 7 days
            }
            else if (record.Status == BookStatus.ReturnRequested)
            {
                record.Status = BookStatus.Submitted;
                record.ReturnedAt = DateTime.Now;
                record.Book.Quantity += 1;
            }

            _context.SaveChanges();
            TempData["Success"] = "✅ Request approved successfully.";
            return RedirectToAction("ManageBookRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectRequest(int id)
        {
            var record = _context.BookRecords.FirstOrDefault(r => r.Id == id);
            if (record == null) return NotFound();

            record.Status = BookStatus.Rejected;
            _context.SaveChanges();

            TempData["Success"] = "❌ Request rejected.";
            return RedirectToAction("ManageBookRequests");
        }


        [HttpGet]
        public IActionResult BrowseBooks(string category = "", string search = "")
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                books = books.Where(b => b.Department.ToLower().Contains(category.ToLower()));
            }

            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b => b.Title.ToLower().Contains(search.ToLower()) || b.Author.ToLower().Contains(search.ToLower()));
            }

            var distinctCategories = _context.Books
                .Select(b => b.Department)
                .Distinct()
                .ToList();

            ViewBag.Categories = distinctCategories;
            ViewBag.SelectedCategory = category;
            ViewBag.SearchQuery = search;

           
         return View(books.ToList());
        }
       
       
        [HttpGet]
        public async Task<IActionResult> MyProfile(bool edit = false)
        {
            string? libId = HttpContext.Session.GetString("LoginId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(libId) || string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            role = role.ToLower();
            if (role != "student" && role != "faculty" && role != "librarian")
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.LoginId == libId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.IsEditMode = edit;

            return View("~/Views/Books/MyProfile.cshtml", user);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User updatedUser)
        {
            var existingUser = await _context.Users.FindAsync(updatedUser.Id);

            if (existingUser == null)
                return NotFound();

            // Update only allowed fields
            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;
            existingUser.Phone = updatedUser.Phone;
            existingUser.Address = updatedUser.Address;
            existingUser.DateOfBirth = updatedUser.DateOfBirth;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "✔ Profile updated successfully!";
            TempData["ToastType"] = "success";

            return RedirectToAction("MyProfile");
        }

    
         [HttpGet]
        public IActionResult MyRequests()
        {
            string? libId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(libId))
                return RedirectToAction("Login", "Account");

            var requests = _context.BookRequests
                .Where(r => r.LoginId == libId)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View("MyRequests", requests);
        }

        [HttpGet]
        public IActionResult MyBookRequests()
        {
            string? libId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(libId))
                return RedirectToAction("Login", "Account");

            var requests = _context.BookRequests
                .Where(r => r.LoginId == libId)
                .ToList();

            return View("MyBookRequests", requests);
        }




        [HttpGet]
        public IActionResult LibrarianDashboard()
        {
            var model = new LibrarianReportViewModel
            {
                TotalBooks = _context.Books.Count(),
                TotalIssued = _context.BookRecords.Count(br => br.Status == BookStatus.Issued),
                TotalSubmitted = _context.BookRecords.Count(br => br.Status == BookStatus.Submitted),

                PendingRequests = _context.BookRecords.Count(br => br.Status == BookStatus.Pending),
                TotalUsers = _context.Users.Count(),
                TotalStudents = _context.Users.Count(u => u.Role == "Student"),
                TotalFaculty = _context.Users.Count(u => u.Role == "Faculty"),
                TotalLibrarians = _context.Users.Count(u => u.Role == "Librarian"),

                MostIssuedBooks = _context.BookRecords
                    .GroupBy(br => br.Book.Title)
                    .Select(g => new MostIssuedBookViewModel
                    {
                        BookName = g.Key,
                        IssueCount = g.Count()
                    })
                    .OrderByDescending(b => b.IssueCount)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult ApproveRequests()
        {
            var requests = _context.BookRecords
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == BookStatus.Pending ||
                            r.Status == BookStatus.RenewalRequested ||
                            r.Status == BookStatus.ReturnRequested)
                .ToList();

            return View("ApproveRequests", requests);
        }

        public async Task<IActionResult> IssuedBooksReport(int page = 1, int pageSize = 10)
        {
            var issuedBooksQuery = _context.BookRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.Status == BookStatus.Issued || b.Status == BookStatus.Submitted);

            // ✅ Total Counts for Dashboard
            ViewBag.TotalIssued = await issuedBooksQuery.CountAsync();
            ViewBag.TotalReturned = await issuedBooksQuery.CountAsync(b => b.Status == BookStatus.Submitted);
            ViewBag.TotalNotReturned = await issuedBooksQuery.CountAsync(b => b.Status == BookStatus.Issued);

            // ✅ Pagination
            var totalRecords = await issuedBooksQuery.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // ✅ Map to ViewModel (no anonymous type)
            var issuedBooks = await issuedBooksQuery
                .OrderByDescending(b => b.IssuedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new IssuedBookReportViewModel
                {
                    BookId = b.BookId,
                    BookTitle = b.Book.Title,
                    UserName = b.User.FullName,
                    UserRole = b.User.Role,
                    IssuedAt = b.IssuedAt,
                    ReturnedAt = b.ReturnedAt,
                    Status = b.Status == BookStatus.Submitted ? "Returned" : "Not Returned"
                })
                .ToListAsync();

            return View(issuedBooks);
        }



                public IActionResult ExportIssuedBooksToExcel()
        {
            // ✅ EF-safe first, then materialize
            var rawRecords = _context.BookRecords
                .Where(br => br.Status == BookStatus.Issued || br.Status == BookStatus.Submitted)
                .Select(br => new
                {
                    BookTitle = br.Book.Title,
                    UserName = br.User.FullName,
                    UserRole = br.User.Role,
                    br.IssuedAt,
                    br.ReturnedAt,
                    IsReturned = br.Status == BookStatus.Submitted
                })
                .ToList();

            // ✅ Now convert safely
            var records = rawRecords.Select(r => new
            {
                r.BookTitle,
                r.UserName,
                r.UserRole,
                IssuedAt = r.IssuedAt.HasValue ? r.IssuedAt.Value.ToString("dd-MM-yyyy") : "",
                ReturnedAt = r.ReturnedAt.HasValue ? r.ReturnedAt.Value.ToString("dd-MM-yyyy") : "-",
                Status = r.IsReturned ? "Returned" : "Not Returned"
            }).ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("IssuedBooks");
            worksheet.Cell(1, 1).InsertTable(records);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "IssuedBooksReport.xlsx");
        }

                // GET: /Books/Renew/5
        [HttpGet]
        public IActionResult Renew(int id)
        {
            var record = _context.BookRecords.FirstOrDefault(br => br.Id == id);
            if (record == null) return NotFound();

            var request = new RenewalRequest
            {
                BookRecordId = record.Id,
                UserId = int.Parse(HttpContext.Session.GetString("UserId")!)

            };

            return View("Renew", request);
        }

        // POST: /Books/RenewBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RenewBook(RenewalRequest request)
        {
            if (!ModelState.IsValid) return View("Renew", request);

            request.RequestedAt = DateTime.Now;
            request.IsApproved = null;  // null = pending
            _context.RenewalRequests.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "🔁 Renewal request submitted successfully!";
            return RedirectToAction("IssuedBooks");
        }

        [HttpGet]
        public IActionResult RenewalRequest()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Librarian") return RedirectToAction("Login", "Account");

            var requests = _context.BookRecords
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == BookStatus.RenewalRequested)
                .ToList();

            return View("RenewalRequests", requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveRenewal(int id)
        {
            var record = _context.BookRecords
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == id);

            if (record == null) return NotFound();

            // Approve renewal by extending DueAt
            record.Status = BookStatus.Issued;
            record.DueAt = record.DueAt?.AddDays(7); // Extend due date
            record.RenewedAt = DateTime.Now;
            record.Notified = true;

            _context.SaveChanges();

            TempData["Success"] = "Renewal approved successfully!";
            return RedirectToAction("RenewalRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectRenewal(int id)
        {
            var record = _context.BookRecords.FirstOrDefault(r => r.Id == id);

            if (record == null) return NotFound();

            record.Status = BookStatus.Rejected;
            record.Notified = true;

            _context.SaveChanges();

            TempData["Error"] = "Renewal request rejected.";
            return RedirectToAction("RenewalRequests");
        }

    }
}