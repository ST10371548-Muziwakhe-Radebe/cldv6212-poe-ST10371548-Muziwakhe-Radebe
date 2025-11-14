using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;

// DESCRIPTION: Azure Function to handle file uploads to Azure Blob Storage.
//              Accepts a file via HTTP POST and uploads it to a specified blob container.
// SOURCES:
//    - Azure Blob Storage Documentation: https://learn.microsoft.com/en-us/azure/storage/blobs/
//    - Azure Functions HTTP Trigger Documentation: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook


public static class BlobFunction
{
    [FunctionName("WriteToBlob")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Uploading to Blob Storage.");

        var formData = await req.ReadFormAsync();
        var file = req.Form.Files["file"];
        if (file == null) return new BadRequestObjectResult("No file uploaded.");

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var blobClient = new BlobContainerClient(connectionString, "images");
        await blobClient.CreateIfNotExistsAsync();

        var blob = blobClient.GetBlobClient(file.FileName);
        using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, true);

        return new OkObjectResult("File uploaded to blob.");
    }
}
