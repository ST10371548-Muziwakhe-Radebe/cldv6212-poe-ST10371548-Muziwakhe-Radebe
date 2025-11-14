using CloudRetailWebApp.Data;
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net; 
using Microsoft.EntityFrameworkCore; 
using System;

// DESCRIPTION: This controller implements user authentication and registration for the Cloud Retail Web App.
// It uses ASP.NET Core's built-in authentication system with cookie-based authentication
// and Entity Framework Core for database interactions.
// SOURCES:
// - ASP.NET Core Authentication & Authorization: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/
// - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
// - Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
// - BCrypt for Password Hashing: https://www.nuget.org/packages/BCrypt.Net-Next/
// - Azure Table Storage Integration: https://learn.microsoft.com/en-us/azure/storage/tables/

namespace CloudRetailWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public AccountController(ApplicationDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
 
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    await SignInUserAsync(user);

                    TempData["SuccessMessage"] = $"Welcome back, {user.Username}!";

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("AdminHome", "Home");
                    }

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("CustomerHome", "Home");
                }
                else
                {
                    
                    ModelState.AddModelError("", "Invalid username or password.");
                    TempData["ErrorMessage"] = "Invalid username or password. Please try again.";
                }
            }

  
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (usernameExists)
            {
                ModelState.AddModelError(nameof(model.Username), "This username is already in use.");
                return View(model);
            }

            var normalizedRole = string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Customer";

            var user = new User
            {
                Username = model.Username.Trim(),
                Email = model.Email,
                Name = model.FullName,
                Role = normalizedRole,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

     
            if (normalizedRole == "Customer")
            {
                try
                {
                    var nameParts = (model.FullName ?? model.Username).Split(' ', 2);
                    var customer = new CustomerModel
                    {
                        RowKey = user.UserId.ToString(),
                        FirstName = nameParts.Length > 0 ? nameParts[0] : model.Username,
                        LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                        Email = model.Email,
                        Phone = string.Empty, 
                        CreatedAt = DateTime.UtcNow
                    };
                    await _storageService.AddCustomerAsync(customer);
                }
                catch (Exception ex)
                {
                   
                    Console.WriteLine($"Warning: Failed to save customer to Azure Storage: {ex.Message}");
                }
            }

            await SignInUserAsync(user);

            TempData["SuccessMessage"] = $"Account created successfully! Welcome, {user.Username}!";

            return normalizedRole == "Admin"
                ? RedirectToAction("AdminHome", "Home")
                : RedirectToAction("CustomerHome", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "User";
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = $"You have been logged out successfully. Goodbye, {username}!";
            return RedirectToAction("Index", "Home"); 
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        }
    }
}