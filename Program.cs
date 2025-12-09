using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------
// 1) Add MVC
// ---------------------------------------
builder.Services.AddControllersWithViews();

// ---------------------------------------
// 2) Add Database Context
// ---------------------------------------
builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});

// ---------------------------------------
// 3) Add Session
// ---------------------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// ---------------------------------------
// 4) Data Protection FIX for Railway
// ---------------------------------------
var dataProtectionPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/dataprotection-keys");
Directory.CreateDirectory(dataProtectionPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("MyLibrarySystem");

// ---------------------------------------
// 5) SMTP Settings from Environment Variables
// ---------------------------------------
builder.Services.Configure<EmailService>(builder.Configuration.GetSection("EmailSettings"));

// ---------------------------------------
var app = builder.Build();

// ---------------------------------------
// 6) Middleware Pipeline
// ---------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ---------------------------------------
// 7) Route
// ---------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
