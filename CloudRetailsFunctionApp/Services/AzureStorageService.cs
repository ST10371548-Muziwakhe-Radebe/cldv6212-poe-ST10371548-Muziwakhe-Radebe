// DESCRIPTION: Implements IStorageService for CloudRetailsFunctionApp,
//              handling CRUD operations for customers and products,
//              order queue operations, blob storage, and Azure file shares.
// SOURCES:
//    - Azure Storage Tables Documentation: https://learn.microsoft.com/en-us/azure/storage/tables/
//    - Azure Blob Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/blobs/
//    - Azure Queues Documentation: https://learn.microsoft.com/en-us/azure/storage/queues/

using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using CloudRetailsFunctionApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudRetailsFunction.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly string _conn;
        private readonly TableClient _customersTable;
        private readonly TableClient _productsTable;
        private readonly BlobContainerClient _blobContainer;
        private readonly QueueClient _queueClient;
        private readonly ShareClient _shareClient;
        private readonly string _blobContainerName;
        private readonly string _fileShareName;

        // ATTRIBUTION: Constructor to initialize Azure Storage clients and ensure resources exist.
        // SOURCES:
        //    - Dependency Injection in ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
        public AzureStorageService(IConfiguration config)
        {
            _conn = config["Azure:StorageConnectionString"];
            _blobContainerName = config["Azure:BlobContainer"] ?? "productimages";
            _fileShareName = config["Azure:FileShareName"] ?? "contracts";

            _customersTable = new TableClient(_conn, config["Azure:TableCustomers"] ?? "Customers");
            _productsTable = new TableClient(_conn, config["Azure:TableProducts"] ?? "Products");
            _blobContainer = new BlobContainerClient(_conn, _blobContainerName);
            _queueClient = new QueueClient(_conn, config["Azure:QueueOrders"] ?? "ordersqueue");
            _shareClient = new ShareClient(_conn, _fileShareName);

            _customersTable.CreateIfNotExistsAsync().Wait();
            _productsTable.CreateIfNotExistsAsync().Wait();
            _blobContainer.CreateIfNotExistsAsync().Wait();
            _queueClient.CreateIfNotExistsAsync().Wait();
            _shareClient.CreateIfNotExistsAsync().Wait();
        }

        // ATTRIBUTION: CRUD operations for customer records using Azure Table Storage.
        // SOURCES:
        //    - Azure Table Storage CRUD: https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-how-to-use-dotnet
        public async Task AddCustomerAsync(CustomerModel customer)
        {
            var entity = new TableEntity(customer.PartitionKey, customer.RowKey)
            {
                { "FirstName", customer.FirstName },
                { "LastName", customer.LastName },
                { "Email", customer.Email },
                { "Phone", customer.Phone },
                { "CreatedAt", customer.CreatedAt }
            };
            await _customersTable.AddEntityAsync(entity);
        }

        public async Task<List<CustomerModel>> GetCustomersAsync()
        {
            var list = new List<CustomerModel>();
            await foreach (var entity in _customersTable.QueryAsync<TableEntity>())
            {
                list.Add(new CustomerModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    FirstName = entity.GetString("FirstName"),
                    LastName = entity.GetString("LastName"),
                    Email = entity.GetString("Email"),
                    Phone = entity.GetString("Phone"),
                    CreatedAt = entity.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                });
            }
            return list.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public async Task<CustomerModel> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _customersTable.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                var entity = response.Value;
                return new CustomerModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    FirstName = entity.GetString("FirstName"),
                    LastName = entity.GetString("LastName"),
                    Email = entity.GetString("Email"),
                    Phone = entity.GetString("Phone"),
                    CreatedAt = entity.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                };
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task UpdateCustomerAsync(CustomerModel customer)
        {
            var entity = new TableEntity(customer.PartitionKey, customer.RowKey)
            {
                { "FirstName", customer.FirstName },
                { "LastName", customer.LastName },
                { "Email", customer.Email },
                { "Phone", customer.Phone },
                { "CreatedAt", customer.CreatedAt }
            };
            await _customersTable.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _customersTable.DeleteEntityAsync(partitionKey, rowKey);
        }

        // ATTRIBUTION: CRUD operations for product records and blob storage handling.
        // SOURCES:
        //    - Azure Blob Storage CRUD: https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-upload
        public async Task AddProductAsync(ProductModel product, IFormFile imageFile = null)
        {
            if (imageFile != null)
            {
                var blobClient = _blobContainer.GetBlobClient($"{product.RowKey}_{imageFile.FileName}");
                using var stream = imageFile.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);
                product.ImageBlobPath = blobClient.Uri.ToString();
            }

            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                { "Name", product.Name },
                { "Description", product.Description },
                { "Price", product.Price },
                { "ImageBlobPath", product.ImageBlobPath ?? string.Empty },
                { "CreatedAt", product.CreatedAt }
            };
            await _productsTable.AddEntityAsync(entity);
        }

        public async Task<List<ProductModel>> GetProductsAsync()
        {
            var list = new List<ProductModel>();
            await foreach (var entity in _productsTable.QueryAsync<TableEntity>())
            {
                list.Add(new ProductModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Name = entity.GetString("Name"),
                    Description = entity.GetString("Description"),
                    Price = entity.GetDouble("Price") != null ? Convert.ToDecimal(entity.GetDouble("Price")) : 0m,
                    ImageBlobPath = entity.GetString("ImageBlobPath"),
                    CreatedAt = entity.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                });
            }
            return list.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public async Task<ProductModel> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productsTable.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                var entity = response.Value;
                return new ProductModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Name = entity.GetString("Name"),
                    Description = entity.GetString("Description"),
                    Price = entity.GetDouble("Price") != null ? Convert.ToDecimal(entity.GetDouble("Price")) : 0m,
                    ImageBlobPath = entity.GetString("ImageBlobPath"),
                    CreatedAt = entity.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                };
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task UpdateProductAsync(ProductModel product, IFormFile imageFile = null)
        {
            if (imageFile != null)
            {
                var blobClient = _blobContainer.GetBlobClient($"{product.RowKey}_{imageFile.FileName}");
                using var stream = imageFile.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);
                product.ImageBlobPath = blobClient.Uri.ToString();
            }

            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                { "Name", product.Name },
                { "Description", product.Description },
                { "Price", product.Price },
                { "ImageBlobPath", product.ImageBlobPath ?? string.Empty },
                { "CreatedAt", product.CreatedAt }
            };
            await _productsTable.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productsTable.DeleteEntityAsync(partitionKey, rowKey);
        }

        // ATTRIBUTION: Queue handling for order processing using Azure Queue Storage.
        // SOURCES:
        //    - Azure Queue Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction
        public async Task EnqueueOrderAsync(OrderModel order)
        {
            var message = JsonConvert.SerializeObject(order);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
        }

        public async Task<List<OrderModel>> GetQueuedOrdersAsync()
        {
            var list = new List<OrderModel>();
            var peeked = await _queueClient.PeekMessagesAsync(maxMessages: 32);
            foreach (var msg in peeked.Value)
            {
                var bytes = Convert.FromBase64String(msg.MessageText);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var order = JsonConvert.DeserializeObject<OrderModel>(json);
                list.Add(order);
            }
            return list;
        }

        // ATTRIBUTION: Uploads files to Azure File Share.
        // SOURCES:
        //    - Azure Files Documentation: https://learn.microsoft.com/en-us/azure/storage/files/
        public async Task SendFileToFileShareAsync(string fileName, byte[] content)
        {
            var directory = _shareClient.GetRootDirectoryClient();
            var file = directory.GetFileClient(fileName);
            using var ms = new MemoryStream(content);
            await file.CreateAsync(ms.Length);
            await file.UploadRangeAsync(new HttpRange(0, ms.Length), ms);
        }
    }
}
