using CloudRetailWebApp.Data;
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authorization; // For [Authorize(Roles = "Admin")]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For EF Async methods
using System.Collections.Generic;
using System.Linq;

namespace CloudRetailWebApp.Controllers
{
    [Authorize(Roles = "Admin")] // Ensure user is an Admin
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
            // Get statistics for dashboard
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalUsers = await _context.Users.CountAsync();
            
            // Get queued orders via Azure Functions (fallback to direct storage if needed)
            var queuedOrders = await _functionApiService.GetQueueMessagesAsync();
            if (queuedOrders == null || !queuedOrders.Any())
            {
                queuedOrders = await _storageService.GetQueuedOrdersAsync();
            }
            
            // Get contract files via Azure Functions (fallback if needed)
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

        // Action to view all orders (accessible only by Admin)
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User) // Include user info (optional, requires User navigation prop)
                .OrderByDescending(o => o.OrderDate) // Show newest first
                .ToListAsync();
            return View(orders);
        }

        // Action to update order status (accessible only by Admin)
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            // Validate newStatus if necessary (e.g., only allow "Processed", "Cancelled")
            if (newStatus != "Processed" && newStatus != "Cancelled")
            {
                // Log error or return bad request
                return BadRequest("Invalid status update.");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null) // Check if order exists
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Orders"); // Redirect back to the orders list
        }
    }
}