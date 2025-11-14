using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// PURPOSE: Application startup & dependency injection setup.
// CODE CREATED BY STUDENT. Framework pattern based on Microsoft tutorial.
// FOUND AT: https://learn.microsoft.com/aspnet/core/fundamentals/?view=aspnetcore-8.0

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Bind Azure settings from appsettings.json
builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("Azure"));

// 2️⃣ Register HttpClient for calling Functions
builder.Services.AddHttpClient<FunctionApiService>();

// 3️⃣ MVC + custom storage service
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IStorageService, AzureStorageService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customers}/{action=Index}/{id?}");

app.Run();
