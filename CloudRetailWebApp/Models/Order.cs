using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Needed for [ForeignKey]

namespace CloudRetailWebApp.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign Key to User

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // e.g., "Pending", "Processed", "Cancelled"

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Navigation property (optional, for easier data access)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // You might also want an OrderItems collection here if you store item details per order
        // public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}