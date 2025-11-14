using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CloudRetailWebApp.Data; // Add this for ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Add this for AddDbContext
using Microsoft.AspNetCore.Authentication.Cookies; // Add this for cookie auth
using Microsoft.Extensions.Logging; // For ILogger
using System; // For Exception

// PURPOSE: Application startup & dependency injection setup.
// CODE CREATED BY STUDENT. Framework pattern based on Microsoft tutorial.
// FOUND AT: https://learn.microsoft.com/aspnet/core/fundamentals/?view=aspnetcore-8.0  

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Bind Azure settings from appsettings.json
// This binds the section named "Azure" from appsettings.json to the AzureSettings class
builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("Azure"));

// 2️⃣ Register HttpClient for calling Functions (if needed later)
builder.Services.AddHttpClient<FunctionApiService>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["Azure:FunctionBaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
});

// 3️⃣ Register Entity Framework Core DbContext for SQL Database (NEW for Part 3)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer( // UseSqlServer comes from Microsoft.EntityFrameworkCore.SqlServer
        builder.Configuration.GetConnectionString("SqlConnectionString") // Fetch the connection string from ConnectionStrings section in appsettings.json
    ));

// 4️⃣ Configure Authentication using Cookies (NEW for Part 3)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect unauthenticated users here
        options.LogoutPath = "/Account/Logout"; // Redirect after logout
        options.AccessDeniedPath = "/Account/AccessDenied"; // Optional: Redirect unauthorized users
    });

// 5️⃣ Add Authorization (NEW for Part 3)
builder.Services.AddAuthorization(options =>
{
    // Example: Define a policy for Admin-only access
    // Note: We don't set FallbackPolicy here to allow anonymous access by default
    // Individual controllers/actions can require auth using [Authorize] attribute
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// 6️⃣ MVC + custom storage service (Azure Storage - existing from Part 1/2)
builder.Services.AddControllersWithViews();
// Register IStorageService to resolve to AzureStorageService implementation
builder.Services.AddSingleton<IStorageService, AzureStorageService>();

var app = builder.Build();

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // This will create the database and tables if they don't exist
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Log the error - database connection might be failing
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database. Please check your connection string in appsettings.json");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use Authentication and Authorization middleware (NEW for Part 3)
// These must be placed AFTER UseRouting and BEFORE UseEndpoints (or Map... calls)
app.UseAuthentication(); // Add before UseAuthorization
app.UseAuthorization(); // Add after UseAuthentication

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();