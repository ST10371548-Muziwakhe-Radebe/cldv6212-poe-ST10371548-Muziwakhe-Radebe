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
    public class OrderModel
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public List<string> ProductIds { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
