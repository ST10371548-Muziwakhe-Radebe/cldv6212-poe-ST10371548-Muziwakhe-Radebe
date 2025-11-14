
// PURPOSE: Represents configuration values from appsettings.json (Azure section).
// FOUND AT: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options


namespace CloudRetailWebApp.Models
{
    public class AzureSettings
    {
        public string ConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        public string TableNameCustomers { get; set; }
        public string TableNameProducts { get; set; }
        public string QueueName { get; set; }
        public string FileShareName { get; set; }
        public string FunctionAppBaseUrl { get; set; }   // optional if calling your Functions
    }
}
