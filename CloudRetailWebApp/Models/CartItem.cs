using System.ComponentModel.DataAnnotations;

namespace CloudRetailWebApp.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign Key to User

        [Required]
        public string ProductId { get; set; } = null!; // Foreign Key to Product (Table Storage RowKey)

        [Required]
        public int Quantity { get; set; } = 1;

        // Navigation properties (optional)
        public virtual User? User { get; set; }
        // public virtual ProductModel? Product { get; set; } // Link to Table Storage Product (fetched separately)
    }
}