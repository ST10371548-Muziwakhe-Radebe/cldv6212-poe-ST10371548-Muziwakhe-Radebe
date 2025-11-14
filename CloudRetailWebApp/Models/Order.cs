using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace CloudRetailWebApp.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; } 

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; 

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

       
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

      
    }
}