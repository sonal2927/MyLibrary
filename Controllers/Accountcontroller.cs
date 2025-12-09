using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Models.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(LibraryDbContext context, IEmailService emailService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        // Login page (GET)
        [HttpGet]
        public IActionResult Login()
        {
            // Clear Toast only for GET request
            TempData["ToastMessage"] = null;
            TempData["ToastType"] = null;

            return View();
        }

        [HttpPost]
public IActionResult Login(string role, string loginId, string password)
{
    if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(password))
    {
        TempData["ToastMessage"] = "⚠ Please enter all fields.";
        TempData["ToastType"] = "warning";
        return View();
    }

    role = role.Trim().ToLower();
    loginId = loginId.Trim().ToLower();
    password = password.Trim();

    // ADMIN fallback
    if (role == "admin" && loginId == "admin" && password == "admin")
    {
        HttpContext.Session.SetString("UserRole", "Admin");
        HttpContext.Session.SetString("LoginId", "admin");
        return RedirectToAction("UserInfo");
    }

    var user = _context.Users.FirstOrDefault(u =>
        ((u.LoginId ?? "").Trim().ToLower() == loginId ||
         (u.Email ?? "").Trim().ToLower() == loginId) &&
         (u.Role ?? "").Trim().ToLower() == role);

    if (user == null)
    {
        TempData["ToastMessage"] = "❌ Invalid credentials.";
        TempData["ToastType"] = "danger";
        return View();
    }

    if (!user.IsApproved)
    {
        TempData["ToastMessage"] = "⏳ Your account is not approved yet.";
        TempData["ToastType"] = "warning";
        return View();
    }

    if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
    {
        TempData["ToastMessage"] = "❌ Invalid credentials.";
        TempData["ToastType"] = "danger";
        return View();
    }

    // Store UserId in session
    HttpContext.Session.SetInt32("UserId", user.Id);
    HttpContext.Session.SetString("LoginId", user.LoginId ?? "");
    HttpContext.Session.SetString("UserRole", user.Role ?? "");

    return user.Role.ToLower() switch
    {
        "student" => RedirectToAction("StudentDashboard", "Books"),
        "faculty" => RedirectToAction("FacultyDashboard", "Books"),
        "librarian" => RedirectToAction("LibrarianDashboard", "Books"),
        _ => RedirectToAction("Login")
    };
}




        // -------------------- EDIT USER (GET) --------------------
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var issuedBooks = _context.BookRecords
                .Where(r => r.UserId == id && r.ReturnedAt == null)
                .Select(r => r.Book.Title)
                .ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Gender = user.Gender,
                Department = user.Department,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                Role = user.Role
            };

            ViewBag.Books = issuedBooks;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View(model);
        }


       

        // -------------------- USER DETAILS --------------------
        [HttpGet]
        public IActionResult UserDetails(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            var issuedBooks = _context.BookRecords
                .Where(br => br.UserId == id && br.Status == BookStatus.Issued)
                .Select(br => new { br.Book.Title, br.IssuedAt })
                .ToList()
                .Select(br => $"{br.Title} (Issued on: {(br.IssuedAt.HasValue ? br.IssuedAt.Value.ToShortDateString() : "N/A")})")
                .ToList();

            ViewBag.Books = issuedBooks;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "";
            return View(user);
        }


        // -------------------- SOFT DELETE --------------------
        [HttpPost]
        public IActionResult SoftDeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsDeleted = true;
                _context.SaveChanges();
                TempData["Success"] = "User removed successfully.";
            }
            return RedirectToAction("UserInfo");
        }

        // -------------------- LOGOUT --------------------
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "✅ You have been logged out successfully.";
            return RedirectToAction("Login");
        }

    

        // -------------------- REGISTER --------------------
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fix validation errors!";
            return View(model);
        }

        // Check if email already exists
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
        if (emailExists)
        {
            TempData["Error"] = "Email already registered!";
            return View(model);
        }

        try
        {
        var user = new User
        {
            FullName = model.FullName,
            Role = model.Role,
            Department = model.Department,
            LoginId = model.UserIdentifier,
            Email = model.Email,
            Phone = model.Phone,
            Gender = model.Gender,
            Year = model.Year,
            Semester = model.Semester,
            DateOfBirth = model.DateOfBirth,
            Address = model.Address,
            IsApproved = false,
            IsDeleted = false,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // ⭐ ADD THIS ⭐
        await _emailService.SendEmailAsync(
            user.Email,
            "Registration Successful - Pending Approval",
            $"<p>Hi {user.FullName},</p><p>Your registration was successful! Admin will approve you soon.</p>"
        );
        // ⭐ END ADD ⭐

        TempData["Success"] = "✅ Your request is submitted! Please wait for admin approval.";
        return RedirectToAction("Register");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        TempData["Error"] = "Something went wrong! Please try again later.";
        return View(model);
    }

    }




        // -------------------- REQUEST BOOK --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestBook(int id, int daysNeeded, DateTime fromDate, DateTime toDate)
        {
            string? loginId = HttpContext.Session.GetString("LoginId");

            if (string.IsNullOrEmpty(loginId))
            {
                TempData["ToastMessage"] = "⚠ Please login to request a book.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Login", "Account");
            }

            bool alreadyRequested = _context.BookRecords.Any(r =>
                r.BookId == id &&
                r.LoginId == loginId &&
                r.Status == BookStatus.Pending);

            if (alreadyRequested)
            {
                TempData["ToastMessage"] = "⚠ You've already requested this book.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("IssueBookForm", new { id });
            }

            var request = new BookRecord
            {
                BookId = id,
                LoginId = loginId,
                RequestedAt = DateTime.UtcNow,
                Status = BookStatus.Pending,
                RequestedDays = daysNeeded,
                FromDate = fromDate,
                ToDate = toDate
            };

            _context.BookRecords.Add(request);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "✅ Book request submitted successfully!";
            TempData["ToastType"] = "success";

            return RedirectToAction("IssueBookForm", new { id });
        }

        

        // -------------------- ADMIN APPROVAL --------------------
        [HttpGet]
        public IActionResult AdminApproval()
        {
            ViewBag.Students = _context.Users.Where(u => !u.IsApproved && (u.Role ?? "") == "Student").ToList();
            ViewBag.Faculty = _context.Users.Where(u => !u.IsApproved && (u.Role ?? "") == "Faculty").ToList();
            ViewBag.Librarians = _context.Users.Where(u => !u.IsApproved && (u.Role ?? "") == "Librarian").ToList();
            return View();
        }

        // Approve user -> generate password, hash it, store only hash, email plain password to user (admin not shown)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null && !user.IsApproved)
            {
                user.IsApproved = true;
                user.ApprovedAt = DateTime.UtcNow;

                // Ensure LoginId exists (flexible logic)
                if (string.IsNullOrWhiteSpace(user.LoginId))
                {
                    // Prefer EnrollmentNumber for students, otherwise use Email
                    if ((user.Role ?? "").Equals("Student", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(user.EnrollmentNumber))
                        user.LoginId = user.EnrollmentNumber;
                    else
                        user.LoginId = user.Email;
                }

                // Generate secure random password
                string plainPassword = GenerateSecurePassword(10);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                _context.SaveChanges();

                // Email the generated password to the user (admin does not see it)
                string subject = "Library Account Approved - Credentials";
                string body = $"Hello {user.FullName},<br/><br/>" +
                              $"Your library account has been approved.<br/>" +
                              $"Login ID: <strong>{user.LoginId}</strong><br/>" +
                              $"Password: <strong>{plainPassword}</strong><br/><br/>" +
                              $"Please change your password after first login.<br/><br/>" +
                              $"Thanks,<br/>Library Team";

                // We await but this method signature is sync; use Task.Run to avoid changing signature.
                Task.Run(async () => await _emailService.SendEmailAsync(user.Email, subject, body));

                TempData["Success"] = $"✅ {user.FullName} approved. Credentials emailed to user.";
            }
            return RedirectToAction("AdminApproval");
        }

        // -------------------- DELETE USER REQUEST --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserRequest(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsApproved);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = "❌ User request deleted successfully.";
            }
            return RedirectToAction("AdminApproval");
        }

        // -------------------- USER INFO --------------------
        [HttpGet]
        public IActionResult UserInfo(string role = "All", string search = "")
        {
            var query = _context.Users.AsQueryable();

            if (role != "All")
                query = query.Where(u => (u.Role ?? "") == role);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.FullName ?? "").Contains(search) ||
                    (u.Email ?? "").Contains(search) ||
                    (u.LoginId ?? "").Contains(search));
            }

            ViewBag.SelectedRole = role;
            ViewBag.SearchQuery = search;

            return View(query.ToList());
        }

        // -------------------- LIBRARIAN PROFILE --------------------
        public IActionResult LibrarianProfile(bool edit = false)
        {
            string? loginId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(loginId))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.LoginId == loginId && u.Role == "Librarian");
            if (user == null)
                return NotFound();

            ViewBag.IsEditMode = edit;
            return View("~/Views/Books/Profile.cshtml", user);
        }

        // -------------------- UPLOAD PROFILE PICTURE --------------------
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction("MyProfile", "Books");
            }

            string? loginId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(loginId))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.LoginId == loginId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("MyProfile", "Books");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(profilePicture.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            user.ProfilePicturePath = "/uploads/" + uniqueFileName;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile picture updated successfully.";
            return RedirectToAction("MyProfile", "Books");
        }

        // -------------------- FORGOT PASSWORD --------------------
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Accept loginId (Enrollment/EmployeeId/Email) and new password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string loginId, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(loginId) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["Error"] = "Please fill in all fields.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.LoginId ?? "").Trim().ToLower() == loginId.Trim().ToLower());

            if (user == null)
            {
                TempData["Error"] = "User not found. Please check your Login ID.";
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password reset successfully. Please login.";
            return RedirectToAction("Login");
        }

        // -------------------- REMOVE PROFILE PICTURE --------------------
        [HttpPost]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            string? loginId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(loginId))
            {
                TempData["Error"] = "User not logged in.";
                return RedirectToAction("Profile");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.LoginId == loginId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Profile");
            }

            // Delete image from wwwroot
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('~', '/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            user.ProfilePicturePath = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile picture removed successfully.";
            return RedirectToAction("Profile");
        }

        // -------------------- UPDATE PROFILE --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            if (!ModelState.IsValid)
                return View("MyProfile", model);

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("MyProfile","Books");
        }

        // -------------------- CHANGE PASSWORD --------------------
      [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            string? loginId = HttpContext.Session.GetString("LoginId");
            if (string.IsNullOrEmpty(loginId))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.LoginId == loginId);
            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                NewPassword != ConfirmPassword)
            {
                TempData["ToastMessage"] = "Please check all password fields.";
                TempData["ToastType"] = "error"; // or "danger" depending on your toast setup
                return RedirectToAction("MyProfile","Books");
            }

            // Verify old password
            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
            {
                TempData["ToastMessage"] = "Current password is incorrect.";
                TempData["ToastType"] = "error";
                return RedirectToAction("MyProfile","Books");
            }

            // Update new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["success"] = "Password changed successfully!";

            return RedirectToAction("MyProfile","Books");
}


        // -------------------- HELPERS --------------------

        // Generate a secure random password (length configurable)
        private string GenerateSecurePassword(int length = 10)
        {
            const string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var chars = bytes.Select(b => allowed[b % allowed.Length]).ToArray();
            // Ensure password contains at least one digit and one special char
            if (!chars.Any(c => char.IsDigit(c)))
                chars[0] = '7';
            if (!chars.Any(c => !char.IsLetterOrDigit(c)))
                chars[1] = '!';

            return new string(chars);
        }
    }
}
