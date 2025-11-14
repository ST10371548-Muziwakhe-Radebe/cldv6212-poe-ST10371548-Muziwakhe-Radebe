// DESCRIPTION: Defines the Product model for CloudRetailsFunctionApp with properties for storing
//              product details including name, description, price, image blob path, and creation timestamp.
// SOURCES:
//    - Azure Table Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/tables/
//    - C# Decimal Type Documentation: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types

using System;

namespace CloudRetailsFunctionApp.Models
{
    public class ProductModel
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageBlobPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional fields used by HTTP Functions when a file is uploaded as Base64.
        public string ImageFileName { get; set; }
        public string ImageBase64 { get; set; }
    }
}
