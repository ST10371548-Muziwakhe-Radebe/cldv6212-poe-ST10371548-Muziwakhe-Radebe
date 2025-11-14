using System.IO;
using System.Threading.Tasks;
using CloudRetailsFunction.Services;
using CloudRetailsFunctionApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

// DESCRIPTION: HTTP-triggered Azure Function that manages products stored in Azure Table Storage and Blob Storage.
//              Supports CRUD semantics via HTTP verbs.

public class BlobFunction
{
    private readonly IStorageService _storageService;

    public BlobFunction(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [FunctionName("Products")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = "products/{partitionKey?}/{rowKey?}")]
        HttpRequest req,
        string partitionKey,
        string rowKey,
        ILogger log)
    {
        switch (req.Method.ToUpperInvariant())
        {
            case "GET":
                if (!string.IsNullOrWhiteSpace(partitionKey) && !string.IsNullOrWhiteSpace(rowKey))
                {
                    var product = await _storageService.GetProductAsync(partitionKey, rowKey);
                    return product == null ? new NotFoundResult() : new OkObjectResult(product);
                }

                var products = await _storageService.GetProductsAsync();
                return new OkObjectResult(products);

            case "POST":
                var newProduct = await DeserializeAsync<ProductModel>(req);
                if (newProduct == null)
                {
                    return new BadRequestObjectResult("Invalid product payload.");
                }

                await _storageService.AddProductAsync(newProduct);
                return new OkObjectResult(newProduct);

            case "PUT":
                var existingProduct = await DeserializeAsync<ProductModel>(req);
                if (existingProduct == null)
                {
                    return new BadRequestObjectResult("Invalid product payload.");
                }

                existingProduct.PartitionKey = partitionKey ?? existingProduct.PartitionKey ?? "Product";
                existingProduct.RowKey = rowKey ?? existingProduct.RowKey;

                if (string.IsNullOrWhiteSpace(existingProduct.RowKey))
                {
                    return new BadRequestObjectResult("RowKey is required for updates.");
                }

                await _storageService.UpdateProductAsync(existingProduct);
                return new OkObjectResult(existingProduct);

            case "DELETE":
                if (string.IsNullOrWhiteSpace(partitionKey) || string.IsNullOrWhiteSpace(rowKey))
                {
                    return new BadRequestObjectResult("PartitionKey and RowKey are required for delete operations.");
                }

                await _storageService.DeleteProductAsync(partitionKey, rowKey);
                return new OkResult();

            default:
                log.LogWarning("Unsupported HTTP method {Method}", req.Method);
                return new BadRequestObjectResult("Unsupported HTTP verb.");
        }
    }

    private static async Task<T> DeserializeAsync<T>(HttpRequest req)
    {
        var payload = await new StreamReader(req.Body).ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(payload);
    }
}
