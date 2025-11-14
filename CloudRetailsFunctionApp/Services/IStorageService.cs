// DESCRIPTION: Defines the IStorageService interface for CloudRetailsFunctionApp,
//              containing method signatures for CRUD operations on customers and products,
//              order queue handling, and Azure file share operations.
// SOURCES:
//    - Azure Storage Tables Documentation: https://learn.microsoft.com/en-us/azure/storage/tables/
//    - Azure Blob Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/blobs/
//    - Azure Queues Documentation: https://learn.microsoft.com/en-us/azure/storage/queues/
//    - Azure Files Documentation: https://learn.microsoft.com/en-us/azure/storage/files/

using CloudRetailsFunctionApp.Models;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace CloudRetailsFunction.Services
{
    public interface IStorageService
    {
        // DESCRIPTION: Methods for CRUD operations on customer records in Azure Table Storage.
        Task AddCustomerAsync(CustomerModel customer);
        Task<List<CustomerModel>> GetCustomersAsync();
        Task<CustomerModel?> GetCustomerAsync(string partitionKey, string rowKey);
        Task UpdateCustomerAsync(CustomerModel customer);
        Task DeleteCustomerAsync(string partitionKey, string rowKey);

        // DESCRIPTION: Methods for CRUD operations on product records in Azure Table Storage
        //              with optional blob image upload.
        Task AddProductAsync(ProductModel product, IFormFile? imageFile = null);
        Task<List<ProductModel>> GetProductsAsync();
        Task<ProductModel?> GetProductAsync(string partitionKey, string rowKey);
        Task UpdateProductAsync(ProductModel product, IFormFile? imageFile = null);
        Task DeleteProductAsync(string partitionKey, string rowKey);


        // DESCRIPTION: Methods for enqueueing orders and retrieving orders from Azure Queue Storage.
        Task EnqueueOrderAsync(OrderMessageModel order);
        Task<List<OrderMessageModel>> GetQueuedOrdersAsync();


        // DESCRIPTION: Method to upload files to Azure File Share storage.
        Task SendFileToFileShareAsync(string fileName, byte[] content);
        Task<List<string>> ListContractFilesAsync();
        Task<byte[]?> DownloadContractFileAsync(string fileName);
        Task<bool> DeleteContractFileAsync(string fileName);
    }
}
