using CloudRetailWebApp.Data;
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net; // Ensure you have installed BCrypt.Net-Next
using Microsoft.EntityFrameworkCore; // For EF Async methods
using System; // For DateTime

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
                // Find user in the SQL database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                // Check if user exists and if the provided password matches the stored hash
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
                    // If login fails, add an error to the model state
                    ModelState.AddModelError("", "Invalid username or password.");
                    TempData["ErrorMessage"] = "Invalid username or password. Please try again.";
                }
            }

            // If we got this far, something failed, redisplay form
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

            // If user is a Customer, also save to Azure Table Storage
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
                        Phone = string.Empty, // Phone not collected during registration
                        CreatedAt = DateTime.UtcNow
                    };
                    await _storageService.AddCustomerAsync(customer);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail registration - user is already saved to SQL
                    // You might want to log this to a logging service
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
            return RedirectToAction("Index", "Home"); // Redirect to landing page after logout
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