using System.ComponentModel.DataAnnotations;

namespace CloudRetailWebApp.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!; 

        [Required]
        [StringLength(255)] 
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = null!; 

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        
        public virtual ICollection<CartItem>? CartItems { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}