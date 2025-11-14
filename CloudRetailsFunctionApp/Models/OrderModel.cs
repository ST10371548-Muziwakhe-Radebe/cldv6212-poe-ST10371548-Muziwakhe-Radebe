// DESCRIPTION: Defines the Order model for CloudRetailsFunctionApp with properties for storing
//              order details including order ID, customer ID, list of product IDs, total amount,
//              and order date.
// SOURCES:
//    - C# Lists and Collections: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/collections
//    - C# DateTime Structure: https://learn.microsoft.com/en-us/dotnet/api/system.datetime

using System.Collections.Generic;
using System;

namespace CloudRetailsFunctionApp.Models
{
    public class OrderItemModel
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderMessageModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public List<OrderItemModel> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }
}
