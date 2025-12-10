using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.DataProtection;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

// Email Service
builder.Services.AddTransient<IEmailService, EmailService>();

// Database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("LibraryConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    ));

// -------------------- Data Protection & Session --------------------

// Persistent key storage for Railway
var keyPath = "/keys/"; // Make sure this folder exists and is persistent in Railway
if (!Directory.Exists(keyPath))
{
    Directory.CreateDirectory(keyPath);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .SetApplicationName("MyLibrary");

builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // adjust as needed
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

// -------------------------------------------------------------------

var app = builder.Build();

// Run migrations + seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    context.Database.Migrate();
    DbSeeder.Seed(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ❌ Do NOT use HTTPS redirection on Railway
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Railway PORT binding — must be BEFORE routing
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
