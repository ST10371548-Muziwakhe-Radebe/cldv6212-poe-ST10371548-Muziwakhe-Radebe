// FILE: OrdersController.cs
// DESCRIPTION: Controller for managing order-related actions in CloudRetailWebApp.
//              Includes listing queued orders and creating new orders via IStorageService.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

// Controllers/OrdersController.cs
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using CloudRetailWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CloudRetailWebApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IStorageService _storageService;
        private readonly ApplicationDbContext _context;

        public OrdersController(IStorageService storageService, ApplicationDbContext context)
        {
            _storageService = storageService;
            _context = context;
        }

        // GET: Orders - Show orders from SQL database
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim.Value);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Create (Simulate placing an order via queue - this is now handled by CartController.Checkout)
        // You might keep this for manual testing or specific scenarios.
        public IActionResult Create()
        {
            // This might involve creating an OrderMessageModel and sending it to the queue via _storageService.
            // But the main flow should be Cart -> Checkout -> SQL Order -> Queue Message.
            // So, this action might be redundant or used differently.
            // ViewBag.Info = "Orders are now placed via the Cart.";
            // return View(); // Or redirect
            return RedirectToAction("Index"); // Redirect as the main flow is via Cart
        }

        // POST: Orders/Create (Simulate placing an order via queue - now handled by CartController.Checkout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("OrderId,UserId,TotalAmount")] OrderMessageModel orderMessage) // Assuming a message model
        {
            // This logic is now in CartController.Checkout
            // 1. Fetch cart items for UserId from SQL DB
            // 2. Calculate TotalAmount
            // 3. Create Order record in SQL DB
            // 4. Clear Cart items from SQL DB
            // 5. Send OrderMessage (based on SQL Order) to Queue via _storageService
            // For this controller, we just redirect.
            return RedirectToAction("Index");
        }
    }
}