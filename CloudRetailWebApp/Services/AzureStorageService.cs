using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Azure.Storage.Files.Shares;
using CloudRetailWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


// PURPOSE: Implements Azure Storage operations for Tables, Blobs, Queues, and Files.
// CODE WRITTEN BY STUDENT using Microsoft Azure SDK for .NET documentation as reference.
// FOUND AT:
//   https://learn.microsoft.com/azure/storage/tables/table-storage-how-to-use-dotnet
//   https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet
//   https://learn.microsoft.com/azure/storage/queues/storage-dotnet-how-to-use-queues
//   https://learn.microsoft.com/azure/storage/files/storage-dotnet-how-to-use-files

// Services/AzureStorageService.cs

namespace CloudRetailWebApp.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly CloudTable _customerTable;
        private readonly CloudTable _productTable;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly QueueClient _queueClient;
        private readonly ShareClient _shareClient;

        public AzureStorageService(IConfiguration configuration) // Inject IConfiguration
        {
            // Read from Azure section in appsettings.json
            string? storageConnectionString = configuration["Azure:StorageConnectionString"] ?? 
                                              configuration.GetConnectionString("StorageConnectionString"); // Fallback to ConnectionStrings section
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new InvalidOperationException("StorageConnectionString is not configured. Please add it to appsettings.json under 'Azure:StorageConnectionString' or 'ConnectionStrings:StorageConnectionString'.");
            }

            // --- Table Storage Setup ---
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            _customerTable = tableClient.GetTableReference("Customers"); // Use your table name
            _customerTable.CreateIfNotExistsAsync().Wait(); // Ensure table exists
            _productTable = tableClient.GetTableReference("Products"); // Use your table name
            _productTable.CreateIfNotExistsAsync().Wait(); // Ensure table exists

            // --- Blob Storage Setup ---
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient("productimages"); // Use your container name
            // Create container without public access (storage account doesn't allow public access)
            // Images will be accessed via SAS tokens or private URLs if needed
            _blobContainerClient.CreateIfNotExistsAsync().Wait(); // Create if not exists

            // --- Queue Storage Setup ---
            QueueServiceClient queueServiceClient = new QueueServiceClient(storageConnectionString);
            _queueClient = queueServiceClient.GetQueueClient("ordersqueue"); // Use your queue name
            _queueClient.CreateIfNotExistsAsync().Wait(); // Create if not exists

            // --- File Storage Setup ---
            ShareServiceClient shareServiceClient = new ShareServiceClient(storageConnectionString);
            _shareClient = shareServiceClient.GetShareClient("contracts"); // Use your share name
            _shareClient.CreateIfNotExistsAsync().Wait(); // Create if not exists
        }

        // --- Customer Methods (Table Storage) ---
        public async Task InsertCustomerAsync(CustomerEntity customer)
        {
            TableOperation insertOperation = TableOperation.Insert(customer);
            await _customerTable.ExecuteAsync(insertOperation);
        }

        public async Task<CustomerEntity?> GetCustomerEntityAsync(string customerId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>("Customer", customerId); // PartitionKey, RowKey
            TableResult result = await _customerTable.ExecuteAsync(retrieveOperation);
            return result.Result as CustomerEntity; // Cast result
        }

        public async Task<List<CustomerEntity>> GetAllCustomersAsync()
        {
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Customer"));
            var results = new List<CustomerEntity>();
            TableContinuationToken? token = null;
            do
            {
                TableQuerySegment<CustomerEntity> segment = await _customerTable.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);
            return results;
        }

        // --- Product Methods (Table Storage) ---
        public async Task InsertProductAsync(ProductEntity product)
        {
            TableOperation insertOperation = TableOperation.Insert(product);
            await _productTable.ExecuteAsync(insertOperation);
        }

        public async Task<ProductEntity?> GetProductEntityAsync(string productId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<ProductEntity>("Product", productId); // PartitionKey, RowKey
            TableResult result = await _productTable.ExecuteAsync(retrieveOperation);
            return result.Result as ProductEntity; // Cast result
        }

        public async Task<List<ProductEntity>> GetAllProductsAsync()
        {
            TableQuery<ProductEntity> query = new TableQuery<ProductEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Product"));
            var results = new List<ProductEntity>();
            TableContinuationToken? token = null;
            do
            {
                TableQuerySegment<ProductEntity> segment = await _productTable.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);
            return results;
        }

        // --- Blob Storage Methods ---
        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(imageStream, overwrite: true); // Overwrite if exists
            return blobClient.Uri.ToString(); // Return the URL of the uploaded image
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            return await blobClient.DeleteIfExistsAsync();
        }

        // --- Queue Storage Methods ---
        public async Task SendMessageToQueueAsync(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }

        // --- File Storage Methods ---
        public async Task<bool> UploadContractFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var shareDirectoryClient = _shareClient.GetRootDirectoryClient();
                var shareFileClient = shareDirectoryClient.GetFileClient(fileName);
                await shareFileClient.CreateAsync(fileStream.Length);
                await shareFileClient.UploadRangeAsync(new Azure.HttpRange(0, fileStream.Length), fileStream);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteContractFileAsync(string fileName)
        {
            try
            {
                var shareDirectoryClient = _shareClient.GetRootDirectoryClient();
                var shareFileClient = shareDirectoryClient.GetFileClient(fileName);
                return await shareFileClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        // IStorageService Implementation Methods

        // Customer methods
        public async Task AddCustomerAsync(CustomerModel customer)
        {
            var entity = new CustomerEntity(customer.RowKey)
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                CreatedAt = customer.CreatedAt
            };
            await InsertCustomerAsync(entity);
        }

        public async Task<List<CustomerModel>> GetCustomersAsync()
        {
            var entities = await GetAllCustomersAsync();
            return entities.Select(e => new CustomerModel
            {
                PartitionKey = e.PartitionKey,
                RowKey = e.RowKey,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                CreatedAt = e.CreatedAt
            }).ToList();
        }

        async Task<CustomerModel?> IStorageService.GetCustomerAsync(string partitionKey, string rowKey)
        {
            var entity = await GetCustomerEntityAsync(rowKey);
            if (entity == null) return null;
            return new CustomerModel
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                Phone = entity.Phone,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task UpdateCustomerAsync(CustomerModel customer)
        {
            var entity = new CustomerEntity(customer.RowKey)
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                CreatedAt = customer.CreatedAt
            };
            TableOperation replaceOperation = TableOperation.Replace(entity);
            await _customerTable.ExecuteAsync(replaceOperation);
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            var entity = await GetCustomerEntityAsync(rowKey);
            if (entity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await _customerTable.ExecuteAsync(deleteOperation);
            }
        }

        // Product methods
        public async Task AddProductAsync(ProductModel product, IFormFile? imageFile = null)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                using var stream = imageFile.OpenReadStream();
                product.ImageBlobPath = await UploadImageAsync(stream, imageFile.FileName);
            }

            var entity = new ProductEntity(product.RowKey)
            {
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                ImageBlobPath = product.ImageBlobPath,
                CreatedAt = product.CreatedAt
            };
            await InsertProductAsync(entity);
        }

        public async Task<List<ProductModel>> GetProductsAsync()
        {
            var entities = await GetAllProductsAsync();
            return entities.Select(e => new ProductModel
            {
                PartitionKey = e.PartitionKey,
                RowKey = e.RowKey,
                Name = e.Name,
                Description = e.Description,
                Price = (decimal)e.Price,
                ImageBlobPath = e.ImageBlobPath,
                CreatedAt = e.CreatedAt
            }).ToList();
        }

        async Task<ProductModel?> IStorageService.GetProductAsync(string partitionKey, string rowKey)
        {
            var entity = await GetProductEntityAsync(rowKey);
            if (entity == null) return null;
            return new ProductModel
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                Name = entity.Name,
                Description = entity.Description,
                Price = (decimal)entity.Price,
                ImageBlobPath = entity.ImageBlobPath,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task UpdateProductAsync(ProductModel product, IFormFile? imageFile = null)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete old image if exists
                var existingProduct = await ((IStorageService)this).GetProductAsync("Product", product.RowKey);
                if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageBlobPath))
                {
                    var fileName = Path.GetFileName(existingProduct.ImageBlobPath);
                    await DeleteImageAsync(fileName);
                }
                // Upload new image
                using var stream = imageFile.OpenReadStream();
                product.ImageBlobPath = await UploadImageAsync(stream, imageFile.FileName);
            }

            var entity = new ProductEntity(product.RowKey)
            {
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                ImageBlobPath = product.ImageBlobPath,
                CreatedAt = product.CreatedAt
            };
            TableOperation replaceOperation = TableOperation.Replace(entity);
            await _productTable.ExecuteAsync(replaceOperation);
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            var entity = await GetProductEntityAsync(rowKey);
            if (entity != null)
            {
                // Delete associated image if exists
                if (!string.IsNullOrEmpty(entity.ImageBlobPath))
                {
                    var fileName = Path.GetFileName(entity.ImageBlobPath);
                    await DeleteImageAsync(fileName);
                }
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await _productTable.ExecuteAsync(deleteOperation);
            }
        }

        // Order methods (Queue Storage)
        public async Task EnqueueOrderAsync(OrderMessageModel order)
        {
            var orderMessage = JsonConvert.SerializeObject(order);
            await SendMessageToQueueAsync(orderMessage);
        }

        public async Task<List<OrderMessageModel>> GetQueuedOrdersAsync()
        {
            // Note: Queue storage is typically for processing, not for listing all messages
            // This is a simplified implementation for demonstration
            var messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 32);
            var orders = new List<OrderMessageModel>();
            foreach (var message in messages.Value)
            {
                try
                {
                    var order = JsonConvert.DeserializeObject<OrderMessageModel>(message.MessageText);
                    if (order != null)
                    {
                        orders.Add(order);
                    }
                }
                catch
                {
                    // Skip invalid messages
                }
            }
            return orders;
        }

        // Contract methods (File Storage)
        public async Task SendFileToFileShareAsync(string fileName, byte[] content)
        {
            using var stream = new MemoryStream(content);
            await UploadContractFileAsync(stream, fileName);
        }

        public async Task<byte[]?> GetFileFromFileShareAsync(string fileName)
        {
            try
            {
                var shareDirectoryClient = _shareClient.GetRootDirectoryClient();
                var shareFileClient = shareDirectoryClient.GetFileClient(fileName);
                
                if (!await shareFileClient.ExistsAsync())
                {
                    return null;
                }

                var fileInfo = await shareFileClient.GetPropertiesAsync();
                var downloadInfo = await shareFileClient.DownloadAsync();
                using var memoryStream = new MemoryStream();
                await downloadInfo.Value.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> ListFilesInFileShareAsync()
        {
            try
            {
                var shareDirectoryClient = _shareClient.GetRootDirectoryClient();
                var files = new List<string>();
                
                await foreach (var fileItem in shareDirectoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!fileItem.IsDirectory)
                    {
                        files.Add(fileItem.Name);
                    }
                }
                
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing files: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> DeleteFileFromFileShareAsync(string fileName)
        {
            return await DeleteContractFileAsync(fileName);
        }

        // Convenience methods for controllers that use single-parameter calls
        public async Task<CustomerEntity?> GetCustomerAsync(string rowKey)
        {
            return await GetCustomerEntityAsync(rowKey);
        }

        public async Task<ProductEntity?> GetProductAsync(string rowKey)
        {
            return await GetProductEntityAsync(rowKey);
        }
    }
}