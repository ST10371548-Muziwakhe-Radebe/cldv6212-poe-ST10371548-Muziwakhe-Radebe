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

// DESCRIPTION: HTTP-triggered Azure Function that exposes CRUD operations for customers stored in Azure Table Storage.
// SOURCES:
//    - Azure Functions HTTP Trigger Documentation: https://learn.microsoft.com/azure/azure-functions/functions-bindings-http-webhook
//    - Azure Table Storage Documentation: https://learn.microsoft.com/azure/storage/tables/

public class TableFunction
{
    private readonly IStorageService _storageService;

    public TableFunction(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [FunctionName("Customers")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = "customers/{partitionKey?}/{rowKey?}")]
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
                    var customer = await _storageService.GetCustomerAsync(partitionKey, rowKey);
                    return customer == null ? new NotFoundResult() : new OkObjectResult(customer);
                }

                var customers = await _storageService.GetCustomersAsync();
                return new OkObjectResult(customers);

            case "POST":
                var newCustomer = await DeserializeAsync<CustomerModel>(req);
                if (newCustomer == null)
                {
                    return new BadRequestObjectResult("Invalid customer payload.");
                }

                if (string.IsNullOrWhiteSpace(newCustomer.PartitionKey))
                {
                    newCustomer.PartitionKey = "Customer";
                }

                if (string.IsNullOrWhiteSpace(newCustomer.RowKey))
                {
                    newCustomer.RowKey = Guid.NewGuid().ToString();
                }

                if (newCustomer.CreatedAt == default)
                {
                    newCustomer.CreatedAt = DateTime.UtcNow;
                }

                await _storageService.AddCustomerAsync(newCustomer);
                return new OkObjectResult(newCustomer);

            case "PUT":
                var updateCustomer = await DeserializeAsync<CustomerModel>(req);
                if (updateCustomer == null)
                {
                    return new BadRequestObjectResult("Invalid customer payload.");
                }

                updateCustomer.PartitionKey = partitionKey ?? updateCustomer.PartitionKey ?? "Customer";
                updateCustomer.RowKey = rowKey ?? updateCustomer.RowKey;

                if (string.IsNullOrWhiteSpace(updateCustomer.RowKey))
                {
                    return new BadRequestObjectResult("RowKey is required for updates.");
                }

                await _storageService.UpdateCustomerAsync(updateCustomer);
                return new OkObjectResult(updateCustomer);

            case "DELETE":
                var deletePartition = partitionKey;
                var deleteRow = rowKey;

                if (string.IsNullOrWhiteSpace(deletePartition) || string.IsNullOrWhiteSpace(deleteRow))
                {
                    return new BadRequestObjectResult("PartitionKey and RowKey are required for delete operations.");
                }

                await _storageService.DeleteCustomerAsync(deletePartition, deleteRow);
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