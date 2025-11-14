
// DESCRIPTION: Controller for managing order-related actions in CloudRetailWebApp.
//              Includes listing queued orders and creating new orders via IStorageService.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
//    - Azure Storage Queues: https://learn.microsoft.com/en-us/azure/storage/queues/


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


        public IActionResult Create()
        {
            return RedirectToAction("Index"); 
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("OrderId,UserId,TotalAmount")] OrderMessageModel orderMessage) // Assuming a message model
        {
            return RedirectToAction("Index");
        }
    }
}