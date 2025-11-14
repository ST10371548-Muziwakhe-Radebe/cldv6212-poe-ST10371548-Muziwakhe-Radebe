// DESCRIPTION: Controller for managing customer-related actions in CloudRetailWebApp.
//              Includes CRUD operations for customers and uses IStorageService for
//              storage interactions.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding and Validation: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

// Controllers/CustomersController.cs
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services; // For IStorageService
using Microsoft.AspNetCore.Mvc;

namespace CloudRetailWebApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IStorageService _storageService; // Inject the service

        public CustomersController(IStorageService storageService) // Constructor injection
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _storageService.GetCustomersAsync(); // Call service method
            return View(customers);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartitionKey,RowKey,FirstName,LastName,Email,Phone")] CustomerModel customer) // Assuming CustomerModel matches your entity
        {
            if (ModelState.IsValid)
            {
                customer.PartitionKey = "Customer"; // Set partition key
                await _storageService.AddCustomerAsync(customer); // Call service method
                TempData["SuccessMessage"] = $"Customer '{customer.FirstName} {customer.LastName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(customer);
        }

        // Add other actions like Details, Edit, Delete as needed, calling corresponding service methods.
        // Example: Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey); // Assuming RowKey is the ID
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
        // Example: Edit (GET)
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey); // Assuming RowKey is the ID
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // Example: Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, [Bind("RowKey,Name,Email,Phone")] CustomerModel customer) // Bind only editable props
        {
            if (rowKey != customer.RowKey) // Ensure the ID in the form matches the route
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Assuming you have an UpdateCustomerAsync method in your service
                    // Or you might need to Delete and Re-Insert, as Table Storage doesn't have a direct "Update" for all properties
                    // For simplicity, let's assume Update is possible via Replace.
                    customer.PartitionKey = "Customer"; // Ensure correct partition
                    // You might need a specific method like UpdateCustomerAsync in AzureStorageService
                    await _storageService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = $"Customer '{customer.FirstName} {customer.LastName}' updated successfully!";
                }
                catch (Exception ex) // Catch potential concurrency errors or other issues
                {
                    // Log the error
                    Console.WriteLine($"Error updating customer: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact your system administrator.");
                    TempData["ErrorMessage"] = "Unable to save changes. Please try again.";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // Example: Delete (GET)
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey); // Assuming RowKey is the ID
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // Example: Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _storageService.GetCustomerAsync("Customer", rowKey);
                var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Customer";
                await _storageService.DeleteCustomerAsync("Customer", rowKey); // Assuming a delete method exists in your service
                TempData["SuccessMessage"] = $"Customer '{customerName}' deleted successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting customer. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
