using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Email Service (won’t crash if SMTP missing)
builder.Services.AddTransient<IEmailService, EmailService>();

// ✅ Database Context
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("LibraryConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    ));

// Middleware
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ Safe DB Migrate + Seed (no crash even if DB empty / duplicate)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<LibraryDbContext>();

        context.Database.Migrate();

        // Safe seeding
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB Migration/Seed Failed: " + ex.Message);
    }
}

// Error handling
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

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Correct Railway port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// RUN SERVER
app.Run();
