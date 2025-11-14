// DESCRIPTION: Controller for managing customer-related actions in CloudRetailWebApp.
//              Includes CRUD operations for customers and uses IStorageService for
//              storage interactions.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding and Validation: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudRetailWebApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IStorageService _storage;
        public CustomersController(IStorageService storage) => _storage = storage;

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _storage.GetCustomersAsync();
            return View(customers);
        }

        // GET: Customers/Create
        public IActionResult Create() => View();

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _storage.AddCustomerAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _storage.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _storage.UpdateCustomerAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _storage.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customers/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _storage.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var customer = await _storage.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null) return NotFound();
            return View(customer);
        }
    }
}
