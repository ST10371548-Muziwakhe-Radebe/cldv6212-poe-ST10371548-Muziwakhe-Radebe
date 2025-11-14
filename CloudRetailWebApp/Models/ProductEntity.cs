using Microsoft.Azure.Cosmos.Table;
using System;

namespace CloudRetailWebApp.Models
{
    public class ProductEntity : TableEntity
    {
        public ProductEntity()
        {
            PartitionKey = "Product";
        }

        public ProductEntity(string productId) : this()
        {
            RowKey = productId;
        }

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public double Price { get; set; } 
        public string? ImageBlobPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

