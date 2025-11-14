// DESCRIPTION: Defines the Customer model for CloudRetailsFunctionApp with properties for storing
//              customer details including name, email, phone, and creation timestamp.
// SOURCES:
//    - Azure Table Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/tables/
//    - C# DateTime Structure: https://learn.microsoft.com/en-us/dotnet/api/system.datetime

using System;

namespace CloudRetailsFunctionApp.Models
{
    public class CustomerModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
