using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using System.IO;
using LibraryManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Logging --------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// -------------------- MVC --------------------
builder.Services.AddControllersWithViews();

// -------------------- Database Context --------------------
builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});

// -------------------- Session --------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// -------------------- Data Protection --------------------
var dataProtectionPath = Path.Combine(Directory.GetCurrentDirectory(), "dataprotection-keys");
Directory.CreateDirectory(dataProtectionPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("MyLibrarySystem");

// -------------------- SMTP / Email --------------------
builder.Services.Configure<EmailService>(builder.Configuration.GetSection("EmailSettings"));

// -------------------- Build App --------------------
var app = builder.Build();

// -------------------- Middleware --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// On Railway, HTTPS may break container, so redirect only in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// -------------------- Default Route --------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
