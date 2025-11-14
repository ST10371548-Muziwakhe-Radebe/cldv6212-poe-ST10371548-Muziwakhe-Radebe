using System;
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

// DESCRIPTION: Azure Function exposing queue operations for order processing.

public class QueueFunction
{
    private readonly IStorageService _storageService;

    public QueueFunction(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [FunctionName("OrdersQueue")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orders/queue")] HttpRequest req,
        ILogger log)
    {
        if (req.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            var orders = await _storageService.GetQueuedOrdersAsync();
            return new OkObjectResult(orders);
        }

        if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            var payload = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(payload))
            {
                return new BadRequestObjectResult("Order payload is required.");
            }

            var order = JsonConvert.DeserializeObject<OrderMessageModel>(payload);
            if (order == null)
            {
                return new BadRequestObjectResult("Invalid order payload.");
            }

            await _storageService.EnqueueOrderAsync(order);
            return new OkObjectResult(order);
        }

        log.LogWarning("Unsupported HTTP method {Method}", req.Method);
        return new BadRequestObjectResult("Unsupported HTTP verb.");
    }
}