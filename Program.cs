using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Email Service Optional
builder.Services.AddTransient<IEmailService, EmailService>();

// Database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("LibraryConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    ));

builder.Services.AddAuthorization();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrations + Seed
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Railway PORT binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// ❗ THIS FIXES THE 502 ERROR
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
