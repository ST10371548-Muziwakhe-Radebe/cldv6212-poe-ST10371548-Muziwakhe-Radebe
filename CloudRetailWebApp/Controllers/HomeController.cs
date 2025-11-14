using System;
using System.Linq;
using System.Security.Claims;
using CloudRetailWebApp.Data;
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// DESCRIPTION: This controller handles the main landing page and user portal redirection
// for the Cloud Retail Web App. It provides separate dashboards for
// customers and administrators based on their role.
// SOURCES:
// - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
// - Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
// - Azure Storage Queues: https://learn.microsoft.com/en-us/azure/storage/queues/
// - Azure Storage Files: https://learn.microsoft.com/en-us/azure/storage/files/storage-dotnet-how-to-use-files

namespace CloudRetailWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, IStorageService storageService, ILogger<HomeController> logger)
        {
            _context = context;
            _storageService = storageService;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction(nameof(Portal));
            }

            return View();
        }

        [Authorize]
        public IActionResult Portal()
        {
            return User.IsInRole("Admin")
                ? RedirectToAction(nameof(AdminHome))
                : RedirectToAction(nameof(CustomerHome));
        }

        [Authorize]
        public async Task<IActionResult> CustomerHome()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdValue, out var userId);

            var orderCount = await _context.Orders.CountAsync(o => o.UserId == userId);
            var cartItems = await _context.CartItems.CountAsync(c => c.UserId == userId);

            var featuredProducts = new List<ProductModel>();
            try
            {
                var products = await _storageService.GetProductsAsync();
                featuredProducts = products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load featured products for customer dashboard.");
            }

            var model = new CustomerDashboardViewModel
            {
                Username = User.Identity?.Name ?? "Customer",
                OrderCount = orderCount,
                CartItems = cartItems,
                FeaturedProducts = featuredProducts
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminHome()
        {
            var model = new AdminDashboardViewModel();

            model.TotalOrders = await _context.Orders.CountAsync();
            model.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            model.TotalUsers = await _context.Users.CountAsync();

            try
            {
                var products = await _storageService.GetProductsAsync();
                model.ProductCount = products.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load product count for admin dashboard.");
            }

            try
            {
                model.QueuedOrders = await _storageService.GetQueuedOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load queue information for admin dashboard.");
            }

            try
            {
                model.ContractFiles = await _storageService.ListFilesInFileShareAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load contract files for admin dashboard.");
            }

            return View(model);
        }
    }
}

