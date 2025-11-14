using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;
using System;

// DESCRIPTION: Azure Function that handles file uploads to Azure File Share storage.
//              Accepts a file via HTTP POST and uploads it to a specified Azure File Share.
// SOURCES:
//    - Azure Files Documentation: https://learn.microsoft.com/en-us/azure/storage/files/
//    - Azure Functions HTTP Trigger Documentation: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook


public static class FileFunction
{
    [FunctionName("WriteToFile")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Uploading to Azure Files.");

        var formData = await req.ReadFormAsync();
        var file = req.Form.Files["file"];
        if (file == null) return new BadRequestObjectResult("No file uploaded.");

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var shareClient = new ShareClient(connectionString, "contracts");
        await shareClient.CreateIfNotExistsAsync();

        var directoryClient = shareClient.GetRootDirectoryClient();
        var fileClient = directoryClient.GetFileClient(file.FileName);
        using var stream = file.OpenReadStream();
        await fileClient.CreateAsync(stream.Length);
        await fileClient.UploadRangeAsync(new Azure.HttpRange(0, stream.Length), stream);

        return new OkObjectResult("File uploaded to Azure Files.");
    }
}