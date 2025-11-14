using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using System;

// DESCRIPTION: Azure Function that stores incoming JSON data to Azure Table Storage.
//              Accepts HTTP POST requests containing partitionKey, rowKey, and name,
//              then inserts the entity into the specified table.
// SOURCES:
//    - Azure Table Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/tables/
//    - Azure Functions HTTP Trigger Documentation: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook
//    - Newtonsoft.Json Documentation: https://www.newtonsoft.com/json/help/html/Introduction.htm

public static class TableFunction
{
    [FunctionName("StoreToTable")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Storing data to Azure Table.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string partitionKey = data?.partitionKey;
        string rowKey = data?.rowKey;
        string name = data?.name;

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableClient = new TableClient(connectionString, "CustomerProfiles");
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity(partitionKey, rowKey)
        {
            { "Name", name }
        };
        await tableClient.AddEntityAsync(entity);

        return new OkObjectResult("Data stored in table.");
    }
}