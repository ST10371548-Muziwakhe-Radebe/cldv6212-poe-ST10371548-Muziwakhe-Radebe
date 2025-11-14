// DESCRIPTION: Controller for managing product-related actions in CloudRetailWebApp.
//              Includes listing, creating, editing, deleting, and viewing product details.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - ASP.NET Core Model Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding

using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudRetailWebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IStorageService _storage;

        // ATTRIBUTION: Constructor injection of storage service.
        // SOURCES:
        //    - Dependency Injection in ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
        public ProductsController(IStorageService storage) => _storage = storage;

        // ATTRIBUTION: Retrieves list of all products for display.
        // SOURCES:
        //    - ASP.NET Core Action Methods: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
        public async Task<IActionResult> Index()
        {
            var products = await _storage.GetProductsAsync();
            return View(products);
        }

        // ATTRIBUTION: Returns view to create a new product.
        // SOURCES:
        //    - ASP.NET Core Views: https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview
        public IActionResult Create() => View();

        // ATTRIBUTION: Handles POST request to add a new product.
        // SOURCES:
        //    - ASP.NET Core Model Binding and Validation: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _storage.AddProductAsync(model, imageFile);
            return RedirectToAction(nameof(Index));
        }

        // ATTRIBUTION: Retrieves product for editing.
        // SOURCES:
        //    - ASP.NET Core Routing: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _storage.GetProductAsync(partitionKey, rowKey);
            if (product == null) return NotFound();
            return View(product);
        }

        // ATTRIBUTION: Handles POST request to update a product.
        // SOURCES:
        //    - ASP.NET Core Model Binding and Form Submission: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _storage.UpdateProductAsync(model, imageFile);
            return RedirectToAction(nameof(Index));
        }

        // ATTRIBUTION: Retrieves product for deletion confirmation.
        // SOURCES:
        //    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var product = await _storage.GetProductAsync(partitionKey, rowKey);
            if (product == null) return NotFound();
            return View(product);
        }

        // ATTRIBUTION: Handles POST request to delete a product.
        // SOURCES:
        //    - ASP.NET Core AntiForgery Validation: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _storage.DeleteProductAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }

        // ATTRIBUTION: Retrieves product details.
        // SOURCES:
        //    - ASP.NET Core MVC Controllers and Views: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var product = await _storage.GetProductAsync(partitionKey, rowKey);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
