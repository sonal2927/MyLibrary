using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using System.IO;
using LibraryManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------- 1) Add MVC --------------------
builder.Services.AddControllersWithViews();

// -------------------- 2) Add Database Context --------------------
builder.Services.AddDbContext<LibraryContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});

// -------------------- 3) Add Session --------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// -------------------- 4) Data Protection --------------------
// Use Railway persistent folder /data for keys
var dataProtectionPath = "/data/dataprotection-keys";
Directory.CreateDirectory(dataProtectionPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("MyLibrarySystem");

// -------------------- 5) SMTP Settings --------------------
builder.Services.Configure<EmailService>(builder.Configuration.GetSection("EmailSettings"));

// -------------------- Build App --------------------
var app = builder.Build();

// -------------------- 6) Middleware --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// Session must come before Authorization
app.UseSession();

app.UseAuthorization();

// -------------------- 7) Default Route --------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// -------------------- 8) Run --------------------
app.Run();
