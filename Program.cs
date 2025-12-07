using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data; // For DbSeeder
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Services; // Email Service Namespace

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Email Service
builder.Services.AddTransient<IEmailService, EmailService>();

// ✅ Database Context
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("LibraryConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    ));

// ✅ Middleware Services
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(); // Enables session
builder.Services.AddControllersWithViews();

// ✅ Build app
var app = builder.Build();

// ✅ Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<LibraryDbContext>();

    // Apply migrations automatically
    context.Database.Migrate();

    // Seed initial data
    DbSeeder.Seed(context);
}

// Error Handling
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

// ✅ Bind to Railway port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");


