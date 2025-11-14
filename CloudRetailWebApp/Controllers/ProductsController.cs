// DESCRIPTION: Controller for managing product-related actions in CloudRetailWebApp.
//              Includes listing, creating, editing, deleting, and viewing product details.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    S- Azure Storage Files (for file handling context): https://learn.microsoft.com/en-us/azure/storage/files/storage-dotnet-how-to-use-files/




using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CloudRetailWebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IStorageService _storageService; 

        public ProductsController(IStorageService storageService) 
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _storageService.GetProductsAsync(); 
            return View(products);
        }

      
        public IActionResult Create()
        {
            return View();
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartitionKey,RowKey,Name,Description,Price,Category,ImageBlobPath")] ProductModel product, IFormFile? imageFile) // Assuming ProductModel matches your entity
        {
            if (ModelState.IsValid)
            {
                product.PartitionKey = "Product"; 
                await _storageService.AddProductAsync(product, imageFile); 
                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(product);
        }

 
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _storageService.GetProductAsync(partitionKey, rowKey); 
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _storageService.GetProductAsync(partitionKey, rowKey); 
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, [Bind("RowKey,Name,Description,Price,Category")] ProductModel product, IFormFile? imageFile) // Bind only editable props
        {
            if (rowKey != product.RowKey) 
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.PartitionKey = "Product"; 
                   
                    await _storageService.UpdateProductAsync(product, imageFile);
                    TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully!";
                }
                catch (Exception ex) 
                {

                    Console.WriteLine($"Error updating product: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact your system administrator.");
                    TempData["ErrorMessage"] = "Unable to save changes. Please try again.";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _storageService.GetProductAsync(partitionKey, rowKey); 
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _storageService.GetProductAsync(partitionKey, rowKey);
                var productName = product?.Name ?? "Product";
     
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


        [HttpPost]
        [Authorize]
        public IActionResult AddToCartFromProducts(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return RedirectToAction(nameof(Index));
            }


            return RedirectToAction("AddToCart", "Cart", new { productId = productId, quantity = 1 });
        }
    }
}