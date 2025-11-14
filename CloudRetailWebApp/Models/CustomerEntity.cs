using Microsoft.Azure.Cosmos.Table;
using System;

namespace CloudRetailWebApp.Models
{
    public class CustomerEntity : TableEntity
    {
        public CustomerEntity()
        {
            PartitionKey = "Customer";
        }

        public CustomerEntity(string customerId) : this()
        {
            RowKey = customerId;
        }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

