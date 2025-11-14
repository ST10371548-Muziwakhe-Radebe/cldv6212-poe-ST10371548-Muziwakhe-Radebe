 // DESCRIPTION    : Defines the Product model for CloudRetailWebApp with properties for storing product details including name, description, price, image blob path, and timestamps.

 // SOURCES        :
 //   - Azure Table Storage: https://learn.microsoft.com/en-us/azure/storage/tables/overview
 //    (For PartitionKey and RowKey structure and mapping to Azure Table storage)
 //   - C# Guid Structure: https://learn.microsoft.com/en-us/dotnet/api/system.guid
 //     (For generating unique RowKey values for models)
 //    - ASP.NET Core Data Binding: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding



using System;

namespace CloudRetailWebApp.Models
{
    public class ProductModel
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string ProductId => RowKey;

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageBlobPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
