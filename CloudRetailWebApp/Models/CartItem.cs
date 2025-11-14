using System.ComponentModel.DataAnnotations;

namespace CloudRetailWebApp.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }

        [Required]
        public int UserId { get; set; } 

        [Required]
        public string ProductId { get; set; } = null!; 

        [Required]
        public int Quantity { get; set; } = 1;


        public virtual User? User { get; set; }
        
    }
}