// FILE: OrdersController.cs
// DESCRIPTION: Controller for managing order-related actions in CloudRetailWebApp.
//              Includes listing queued orders and creating new orders via IStorageService.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudRetailWebApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IStorageService _storage;
        public OrdersController(IStorageService storage) => _storage = storage;

        public async Task<IActionResult> Index()
        {
            var orders = await _storage.GetQueuedOrdersAsync();
            return View(orders);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(OrderModel order)
        {
            if (!ModelState.IsValid) return View(order);
            await _storage.EnqueueOrderAsync(order);
            return RedirectToAction(nameof(Index));
        }
    }
}
