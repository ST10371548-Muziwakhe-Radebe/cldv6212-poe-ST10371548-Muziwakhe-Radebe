// DESCRIPTION: Defines the Customer model for CloudRetailWebApp with properties for storing
//              customer details such as name, email, phone, and timestamps for creation.
// SOURCES:
//    - Azure Table Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/tables/
//    - C# Guid Structure: https://learn.microsoft.com/en-us/dotnet/api/system.guid

using System;

namespace CloudRetailWebApp.Models
{
    public class CustomerModel
    {
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId => RowKey;

        public string FirstName { get; set; } = null!; // Use null-forgiving if you ensure it's set
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}