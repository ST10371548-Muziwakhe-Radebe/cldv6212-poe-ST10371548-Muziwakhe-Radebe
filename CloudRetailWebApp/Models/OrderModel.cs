// DESCRIPTION: Defines the Order model for CloudRetailWebApp with properties for order tracking,
//              including customer ID, product ID, quantity, pricing, creation timestamp, and status.
// SOURCES:
//    - C# Guid Structure: https://learn.microsoft.com/en-us/dotnet/api/system.guid
//    - C# Auto-Properties and Expression-Bodied Members: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties

using System;

namespace CloudRetailWebApp.Models
{
    public class OrderModel
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Quantity * AmountPerItem;
        public decimal AmountPerItem { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }
}
