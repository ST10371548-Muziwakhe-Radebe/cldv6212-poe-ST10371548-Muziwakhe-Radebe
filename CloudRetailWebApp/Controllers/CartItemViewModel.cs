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

