using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using System;

// DESCRIPTION: Azure Function that handles sending and receiving messages in Azure Queue Storage.
//              Accepts a message via HTTP POST, enqueues it, then dequeues and logs the message.
// SOURCES:
//    - Azure Queue Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/queues/
//    - Azure Functions HTTP Trigger Documentation: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook

public static class QueueFunction
{
    [FunctionName("QueueTransaction")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Handling queue transaction.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        string message = requestBody.Trim(); // Ensure no extra whitespace

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var queueClient = new QueueClient(connectionString, "orders");
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(message);

        // Receive a message
        var response = await queueClient.ReceiveMessageAsync();
        if (response.Value != null)
        {
            var queueMessage = response.Value;
            log.LogInformation($"Dequeued: {queueMessage.Body}");
            await queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);
        }

        return new OkObjectResult("Queue message handled.");
    }
}