// DESCRIPTION: Controller for managing product-related actions in CloudRetailWebApp.
//              Includes listing, creating, editing, deleting, and viewing product details.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

// Controllers/ProductsController.cs
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services; // For IStorageService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CloudRetailWebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IStorageService _storageService; // Inject the service

        public ProductsController(IStorageService storageService) // Constructor injection
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _storageService.GetProductsAsync(); // Call service method
            return View(products);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartitionKey,RowKey,Name,Description,Price,Category,ImageBlobPath")] ProductModel product, IFormFile? imageFile) // Assuming ProductModel matches your entity
        {
            if (ModelState.IsValid)
            {
                product.PartitionKey = "Product"; // Set partition key
                await _storageService.AddProductAsync(product, imageFile); // Call service method (handles image upload)
                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(product);
        }

        // Add other actions like Details, Edit, Delete as needed, calling corresponding service methods.
        // Example: Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _storageService.GetProductAsync(partitionKey, rowKey); // Assuming RowKey is the ID
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Example: Edit (GET)
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _storageService.GetProductAsync(partitionKey, rowKey); // Assuming RowKey is the ID
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Example: Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, [Bind("RowKey,Name,Description,Price,Category")] ProductModel product, IFormFile? imageFile) // Bind only editable props
        {
            if (rowKey != product.RowKey) // Ensure the ID in the form matches the route
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.PartitionKey = "Product"; // Ensure correct partition
                    // UpdateProductAsync handles image upload/deletion internally
                    await _storageService.UpdateProductAsync(product, imageFile);
                    TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully!";
                }
                catch (Exception ex) // Catch potential concurrency errors or other issues
                {
                    // Log the error
                    Console.WriteLine($"Error updating product: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact your system administrator.");
                    TempData["ErrorMessage"] = "Unable to save changes. Please try again.";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // Example: Delete (GET)
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _storageService.GetProductAsync(partitionKey, rowKey); // Assuming RowKey is the ID
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        // Example: Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _storageService.GetProductAsync(partitionKey, rowKey);
                var productName = product?.Name ?? "Product";
                // DeleteProductAsync handles image deletion internally
                await _storageService.DeleteProductAsync(partitionKey, rowKey);
                TempData["SuccessMessage"] = $"Product '{productName}' deleted successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting product. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Add to Cart from Products page
        [HttpPost]
        [Authorize]
        public IActionResult AddToCartFromProducts(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return RedirectToAction(nameof(Index));
            }

            // Redirect to Cart controller's AddToCart action
            return RedirectToAction("AddToCart", "Cart", new { productId = productId, quantity = 1 });
        }
    }
}