// DESCRIPTION: Controller for managing customer-related actions in CloudRetailWebApp.
//              Includes CRUD operations for customers and uses IStorageService for
//              storage interactions.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding and Validation: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding


using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services; 
using Microsoft.AspNetCore.Mvc;

namespace CloudRetailWebApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IStorageService _storageService;

        public CustomersController(IStorageService storageService) 
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _storageService.GetCustomersAsync(); 
            return View(customers);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartitionKey,RowKey,FirstName,LastName,Email,Phone")] CustomerModel customer) // Assuming CustomerModel matches your entity
        {
            if (ModelState.IsValid)
            {
                customer.PartitionKey = "Customer"; 
                await _storageService.AddCustomerAsync(customer); 
                TempData["SuccessMessage"] = $"Customer '{customer.FirstName} {customer.LastName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(customer);
        }

   
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey); 
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
  
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey); 
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, [Bind("RowKey,Name,Email,Phone")] CustomerModel customer) // Bind only editable props
        {
            if (rowKey != customer.RowKey) 
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                   
                    customer.PartitionKey = "Customer"; 
                   
                    await _storageService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = $"Customer '{customer.FirstName} {customer.LastName}' updated successfully!";
                }
                catch (Exception ex) 
                {
                    
                    Console.WriteLine($"Error updating customer: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact your system administrator.");
                    TempData["ErrorMessage"] = "Unable to save changes. Please try again.";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey); 
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

     
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _storageService.GetCustomerAsync("Customer", rowKey);
                var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Customer";
                await _storageService.DeleteCustomerAsync("Customer", rowKey); 
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
