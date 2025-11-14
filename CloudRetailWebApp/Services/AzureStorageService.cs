using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Azure.Storage.Files.Shares;
using CloudRetailWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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


namespace CloudRetailWebApp.Services
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

        // CONSTRUCTOR
        // PURPOSE: Initializes Azure SDK clients for Table, Blob, Queue, and File Storage.
        // REFERENCES:
        //   https://learn.microsoft.com/azure/storage/common/storage-introduction
        //   https://learn.microsoft.com/dotnet/api/azure.data.tables.tableclient

        public AzureStorageService(IConfiguration config)
        {
            _conn = config["Azure:StorageConnectionString"];
            _blobContainerName = config["Azure:BlobContainer"];
            _fileShareName = config["Azure:FileShareName"];

            _customersTable = new TableClient(_conn, config["Azure:TableCustomers"] ?? "Customers");
            _productsTable = new TableClient(_conn, config["Azure:TableProducts"] ?? "Products");
            _blobContainer = new BlobContainerClient(_conn, _blobContainerName ?? "productimages");
            _queueClient = new QueueClient(_conn, config["Azure:QueueOrders"] ?? "ordersqueue");
            _shareClient = new ShareClient(_conn, _fileShareName ?? "contracts");

            // Ensures resources exist
            _customersTable.CreateIfNotExists();
            _productsTable.CreateIfNotExists();
            _blobContainer.CreateIfNotExists();
            _queueClient.CreateIfNotExists();
            _shareClient.CreateIfNotExists();
        }

        // CUSTOMER STORAGE SECTION (Azure Table Storage)
        // FOUND AT:
        //   https://learn.microsoft.com/azure/storage/tables/table-storage-design-guide


        public async Task AddCustomerAsync(CustomerModel customer)
        {
            // Adds a new entity to Azure Table
            var entity = new TableEntity(customer.PartitionKey, customer.RowKey)
            {
                {"FirstName", customer.FirstName},
                {"LastName", customer.LastName},
                {"Email", customer.Email},
                {"Phone", customer.Phone},
                {"CreatedAt", customer.CreatedAt}
            };
            await _customersTable.AddEntityAsync(entity);
        }

        public async Task<List<CustomerModel>> GetCustomersAsync()
        {
            // Retrieves all entities from Table Storage
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
            // Retrieves one specific customer
            // FOUND AT: https://learn.microsoft.com/dotnet/api/azure.data.tables.tableclient.getentityasync
            try
            {
                var response = await _customersTable.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                var e = response.Value;
                return new CustomerModel
                {
                    PartitionKey = e.PartitionKey,
                    RowKey = e.RowKey,
                    FirstName = e.GetString("FirstName"),
                    LastName = e.GetString("LastName"),
                    Email = e.GetString("Email"),
                    Phone = e.GetString("Phone"),
                    CreatedAt = e.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                };
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task UpdateCustomerAsync(CustomerModel customer)
        {
            // Updates an existing customer entity.
            // FOUND AT: https://learn.microsoft.com/dotnet/api/azure.data.tables.tableupdateoptions
            var entity = new TableEntity(customer.PartitionKey, customer.RowKey)
            {
                {"FirstName", customer.FirstName},
                {"LastName", customer.LastName},
                {"Email", customer.Email},
                {"Phone", customer.Phone},
                {"CreatedAt", customer.CreatedAt}
            };
            await _customersTable.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            // Deletes an entity from Table Storage.
            await _customersTable.DeleteEntityAsync(partitionKey, rowKey);
        }


        // PRODUCT STORAGE SECTION (Table + Blob Storage)
        // FOUND AT:
        //   https://learn.microsoft.com/azure/storage/blobs/storage-upload-process-images
        //   https://learn.microsoft.com/azure/storage/tables/table-storage-how-to-use-dotnet


        public async Task AddProductAsync(ProductModel product, IFormFile imageFile = null)
        {
            // Uploads product image to Blob and adds metadata to Table
            if (imageFile != null)
            {
                var blobClient = _blobContainer.GetBlobClient($"{product.RowKey}_{imageFile.FileName}");
                using var stream = imageFile.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);
                product.ImageBlobPath = blobClient.Uri.ToString();
            }

            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                {"Name", product.Name},
                {"Description", product.Description},
                {"Price", product.Price},
                {"ImageBlobPath", product.ImageBlobPath ?? string.Empty},
                {"CreatedAt", product.CreatedAt}
            };
            await _productsTable.AddEntityAsync(entity);
        }

        public async Task<List<ProductModel>> GetProductsAsync()
        {
            // Retrieves all products
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
            // Retrieves one product entity from Table Storage
            try
            {
                var response = await _productsTable.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                var e = response.Value;
                return new ProductModel
                {
                    PartitionKey = e.PartitionKey,
                    RowKey = e.RowKey,
                    Name = e.GetString("Name"),
                    Description = e.GetString("Description"),
                    Price = e.GetDouble("Price") != null ? Convert.ToDecimal(e.GetDouble("Price")) : 0m,
                    ImageBlobPath = e.GetString("ImageBlobPath"),
                    CreatedAt = e.GetDateTime("CreatedAt") ?? DateTime.UtcNow
                };
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task UpdateProductAsync(ProductModel product, IFormFile imageFile = null)
        {
            // Updates product information and reuploads image if provided
            if (imageFile != null)
            {
                var blobClient = _blobContainer.GetBlobClient($"{product.RowKey}_{imageFile.FileName}");
                using var stream = imageFile.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);
                product.ImageBlobPath = blobClient.Uri.ToString();
            }

            var entity = new TableEntity(product.PartitionKey, product.RowKey)
            {
                {"Name", product.Name},
                {"Description", product.Description},
                {"Price", product.Price},
                {"ImageBlobPath", product.ImageBlobPath ?? string.Empty},
                {"CreatedAt", product.CreatedAt}
            };
            await _productsTable.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            // Deletes product entity from Table Storage
            await _productsTable.DeleteEntityAsync(partitionKey, rowKey);
        }

        // ORDER QUEUE SECTION (Azure Queue Storage)
        // FOUND AT:
        //   https://learn.microsoft.com/azure/storage/queues/storage-dotnet-how-to-use-queues
    

        public async Task EnqueueOrderAsync(OrderModel order)
        {
            // Adds a new message (order) to the Azure Queue
            var message = JsonConvert.SerializeObject(order);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
        }

        public async Task<List<OrderModel>> GetQueuedOrdersAsync()
        {
            // Reads messages from the Azure Queue (peek mode)
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

        // AZURE FILE SHARE SECTION
        // FOUND AT:
        //   https://learn.microsoft.com/azure/storage/files/storage-dotnet-how-to-use-files
       
        public async Task SendFileToFileShareAsync(string fileName, byte[] content)
        {
            // Uploads a file into Azure File Storage
            var share = _shareClient;
            var directory = share.GetRootDirectoryClient();
            var file = directory.GetFileClient(fileName);
            using var ms = new MemoryStream(content);
            await file.CreateAsync(ms.Length);
            await file.UploadRangeAsync(new HttpRange(0, ms.Length), ms);
        }
    }
}
