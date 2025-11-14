using CloudRetailWebApp.Models; 
using System.Collections.Generic; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Azure;
using CloudRetailWebApp.Services;
using static System.Net.WebRequestMethods;
using System.Collections;
using System.Reflection.Metadata;

// DESCRIPTION    : Defines the storage service interface for handling CRUD operations on customers and Products, and queue operations for orders within the CloudRetailWebApp system.

// SOURCES        :
// Azure Documentation: https://learn.microsoft.com/en-us/azure/storage/
// Microsoft Docs on Azure Table Storage: https://learn.microsoft.com/en-us/azure/storage/tables/
// Microsoft Docs on Azure Blob Storage: https://learn.microsoft.com/en-us/azure/storage/blobs/
// Microsoft Docs on Azure Queue Storage: https://learn.microsoft.com/en-us/azure/storage/queues/
// ASP.NET Core IFormFile Interface: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformfile


namespace CloudRetailWebApp.Services
{
    public interface IStorageService
    {
       
        Task AddCustomerAsync(CustomerModel customer);
        Task<List<CustomerModel>> GetCustomersAsync();
        Task<CustomerModel?> GetCustomerAsync(string partitionKey, string rowKey); 
        Task UpdateCustomerAsync(CustomerModel customer);
        Task DeleteCustomerAsync(string partitionKey, string rowKey);

     
        Task AddProductAsync(ProductModel product, IFormFile? imageFile = null); 
        Task<List<ProductModel>> GetProductsAsync();
        Task<ProductModel?> GetProductAsync(string partitionKey, string rowKey); 
        Task UpdateProductAsync(ProductModel product, IFormFile? imageFile = null); 
        Task DeleteProductAsync(string partitionKey, string rowKey);

        Task EnqueueOrderAsync(OrderMessageModel order);
        Task<List<OrderMessageModel>> GetQueuedOrdersAsync();

     
        Task SendFileToFileShareAsync(string fileName, byte[] content);
        Task<byte[]?> GetFileFromFileShareAsync(string fileName);
        Task<List<string>> ListFilesInFileShareAsync();
        Task<bool> DeleteFileFromFileShareAsync(string fileName);
    }
}