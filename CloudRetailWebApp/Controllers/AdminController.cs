using CloudRetailWebApp.Data;
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Linq;

// DESCRIPTION: This controller handles administrative functions for the Cloud Retail Web App.
// It enforces role-based access control for administrators only and provides
// dashboards, order management, and status updates.
// SOURCES:
// - ASP.NET Core Authorization: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
// - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
// - Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
// - Azure Functions: https://learn.microsoft.com/en-us/azure/azure-functions/
// - Azure Storage Queues: https://learn.microsoft.com/en-us/azure/storage/queues/
// - Azure Storage Files: https://learn.microsoft.com/en-us/azure/storage/files/storage-dotnet-how-to-use-files

namespace CloudRetailWebApp.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;
        private readonly FunctionApiService _functionApiService;

        public AdminController(ApplicationDbContext context, IStorageService storageService, FunctionApiService functionApiService)
        {
            _context = context;
            _storageService = storageService;
            _functionApiService = functionApiService;
        }

        public async Task<IActionResult> Index()
        {

            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalUsers = await _context.Users.CountAsync();
            
      
            var queuedOrders = await _functionApiService.GetQueueMessagesAsync();
            if (queuedOrders == null || !queuedOrders.Any())
            {
                queuedOrders = await _storageService.GetQueuedOrdersAsync();
            }
            
   
            var contractFiles = await _functionApiService.GetContractFilesAsync();
            if (contractFiles == null || !contractFiles.Any())
            {
                contractFiles = await _storageService.ListFilesInFileShareAsync();
            }

            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.QueuedOrders = queuedOrders?.ToList() ?? new List<OrderMessageModel>();
            ViewBag.ContractFiles = contractFiles?.ToList() ?? new List<string>();

            return View();
        }

   
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User) 
                .OrderByDescending(o => o.OrderDate) 
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            
            if (newStatus != "Processed" && newStatus != "Cancelled")
            {
        
                return BadRequest("Invalid status update.");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null) 
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Orders"); 
        }
    }
}