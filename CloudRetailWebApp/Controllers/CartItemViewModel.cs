using CloudRetailWebApp.Models;

// DESCRIPTION: This model represents the data structure for a cart item combined with its product details
//              for display in the user interface within the Cloud Retail Web App.
//              It is used by the CartController to pass necessary information to the view.
// SOURCES:
//    - ASP.NET Core MVC Models: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - Azure Queue Storage (for application context): https://learn.microsoft.com/en-us/azure/storage/queues/

using CloudRetailWebApp.Models;

namespace CloudRetailWebApp.Controllers
{
    public class CartItemViewModel
    {
        public CartItem CartItem { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string ProductId { get; set; } = null!;
    }
}