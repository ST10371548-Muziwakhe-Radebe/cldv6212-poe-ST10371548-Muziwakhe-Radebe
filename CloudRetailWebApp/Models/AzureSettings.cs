
// PURPOSE: Represents configuration values from appsettings.json (Azure section).
// FOUND AT: https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options


namespace CloudRetailWebApp.Models
{
    public class AzureSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string BlobContainerName { get; set; } = string.Empty;
        public string TableNameCustomers { get; set; } = string.Empty;
        public string TableNameProducts { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string FileShareName { get; set; } = string.Empty;
        public string FunctionAppBaseUrl { get; set; } = string.Empty;   // optional if calling your Functions
    }
}
