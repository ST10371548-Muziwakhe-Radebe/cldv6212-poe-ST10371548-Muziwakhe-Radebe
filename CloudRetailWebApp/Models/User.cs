using System.ComponentModel.DataAnnotations;

namespace CloudRetailWebApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!; // Suppress warning: will be set by EF or code

        [Required]
        [StringLength(255)] // Store hashed password
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = null!; // "Customer" or "Admin"

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        // Navigation properties for related entities (optional, for easier data access)
        public virtual ICollection<CartItem>? CartItems { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}