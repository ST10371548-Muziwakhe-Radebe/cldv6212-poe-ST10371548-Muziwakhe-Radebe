// DESCRIPTION: Defines the Order model for CloudRetailWebApp with properties for order tracking,
//              including customer ID, product ID, quantity, pricing, creation timestamp, and status.
// SOURCES:
//    - C# Guid Structure: https://learn.microsoft.com/en-us/dotnet/api/system.guid
//    - C# Auto-Properties and Expression-Bodied Members: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties

// This model represents the *content* of a message sent to the Azure Queue.
// The actual order *record* will be stored in SQL.

using System;
using System.Collections.Generic;

namespace CloudRetailWebApp.Models
{
    public class OrderItemModel
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderMessageModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; } // Link to SQL User
        public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>(); // Snapshot of cart
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Initial status
    }

    // Model for views that display order information
    public class OrderModel
    {
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal AmountPerItem { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}