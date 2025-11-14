using CloudRetailWebApp.Data;
using CloudRetailWebApp.Models;
using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// DESCRIPTION: This controller handles shopping cart functions for the Cloud Retail Web App.
// It manages adding, removing, and updating items in the user's cart, and
// processes the checkout flow by creating orders and placing them in a queue.
// SOURCES:
// - ASP.NET Core Authorization: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/
// - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
// - Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
// - Azure Storage Queues: https://learn.microsoft.com/en-us/azure/storage/queues/

namespace CloudRetailWebApp.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public CartController(ApplicationDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var cartItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            var viewModels = new List<CartItemViewModel>();
            foreach (var cartItem in cartItems)
            {
                var product = await _storageService.GetProductAsync("Product", cartItem.ProductId);
                viewModels.Add(new CartItemViewModel
                {
                    CartItem = cartItem,
                    ProductId = cartItem.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    ProductDescription = product?.Description,
                    ProductPrice = product?.Price ?? 0,
                    ImageUrl = product?.ImageBlobPath
                });
            }

            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            if (quantity <= 0 || string.IsNullOrEmpty(productId))
            {
                TempData["ErrorMessage"] = "Invalid product or quantity.";
                return RedirectToAction("Index", "Products");
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var product = await _storageService.GetProductAsync("Product", productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                TempData["SuccessMessage"] = $"Updated quantity of {product.Name} in your cart.";
            }
            else
            {
                var newCartItem = new CartItem { UserId = userId, ProductId = productId, Quantity = quantity };
                _context.CartItems.Add(newCartItem);
                TempData["SuccessMessage"] = $"{product.Name} added to cart successfully!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId);

            if (cartItem != null)
            {
                var product = await _storageService.GetProductAsync("Product", cartItem.ProductId);
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{product?.Name ?? "Item"} removed from cart.";
            }
            else
            {
                TempData["ErrorMessage"] = "Cart item not found.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                return await RemoveFromCart(cartItemId);
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity = newQuantity;
                await _context.SaveChangesAsync();
                var product = await _storageService.GetProductAsync("Product", cartItem.ProductId);
                TempData["SuccessMessage"] = $"Updated quantity of {product?.Name ?? "item"} to {newQuantity}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Cart item not found.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var cartItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["WarningMessage"] = "Your cart is empty. Add items before checkout.";
                return RedirectToAction("Index");
            }

            decimal totalAmount = 0;
            var orderItems = new List<OrderItemModel>();
            foreach (var item in cartItems)
            {
                var product = await _storageService.GetProductAsync("Product", item.ProductId);
                if (product != null)
                {
                    totalAmount += product.Price * item.Quantity;
                    orderItems.Add(new OrderItemModel
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            if (orderItems.Any())
            {
                var message = new OrderMessageModel
                {
                    OrderId = order.OrderId,
                    UserId = userId,
                    TotalAmount = totalAmount,
                    OrderDate = order.OrderDate,
                    Status = order.Status ?? "Pending",
                    Items = orderItems
                };

                await _storageService.EnqueueOrderAsync(message);
            }

            TempData["SuccessMessage"] = $"Order #{order.OrderId} placed successfully! Total: ${totalAmount:F2}. Your order is being processed.";
            return RedirectToAction("Index", "Orders");
        }

        // CartView action for viewing selected products
        public async Task<IActionResult> CartView()
        {
            // This is an alias for Index - shows the cart view
            return await Index();
        }
    }
}