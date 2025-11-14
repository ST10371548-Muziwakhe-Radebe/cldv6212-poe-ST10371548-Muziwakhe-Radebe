using System;
using System.IO;
using System.Threading.Tasks;
using CloudRetailsFunction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// DESCRIPTION: Azure Function that manages contract files stored in Azure File Shares.

public class FileFunction
{
    private readonly IStorageService _storageService;

    public FileFunction(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [FunctionName("Contracts")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "contracts/{fileName?}")]
        HttpRequest req,
        string fileName,
        ILogger log)
    {
        if (req.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var files = await _storageService.ListContractFilesAsync();
                return new OkObjectResult(files);
            }

            var bytes = await _storageService.DownloadContractFileAsync(fileName);
            if (bytes == null)
            {
                return new NotFoundResult();
            }

            return new FileContentResult(bytes, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }

        if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            byte[] bytes = null;
            string targetFileName = fileName;

            if (req.HasFormContentType)
            {
                var form = await req.ReadFormAsync();
                var formFile = form.Files["file"];
                if (formFile == null)
                {
                    return new BadRequestObjectResult("A file is required.");
                }

                targetFileName = string.IsNullOrWhiteSpace(form["fileName"]) ? formFile.FileName : form["fileName"].ToString();
                using var ms = new MemoryStream();
                await formFile.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            else
            {
                var payload = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(payload))
                {
                    return new BadRequestObjectResult("Request body is required.");
                }

                var contractRequest = JsonConvert.DeserializeObject<ContractUploadRequest>(payload);
                if (contractRequest == null || string.IsNullOrWhiteSpace(contractRequest.FileName) || string.IsNullOrWhiteSpace(contractRequest.Base64))
                {
                    return new BadRequestObjectResult("Invalid contract payload.");
                }

                targetFileName = contractRequest.FileName;
                bytes = Convert.FromBase64String(NormalizeBase64(contractRequest.Base64));
            }

            if (string.IsNullOrWhiteSpace(targetFileName))
            {
                return new BadRequestObjectResult("File name is required.");
            }

            await _storageService.SendFileToFileShareAsync(targetFileName, bytes);
            return new OkObjectResult(new { fileName = targetFileName });
        }

        if (req.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new BadRequestObjectResult("File name is required.");
            }

            var deleted = await _storageService.DeleteContractFileAsync(fileName);
            return deleted ? new OkResult() : new NotFoundResult();
        }

        log.LogWarning("Unsupported HTTP method {Method}", req.Method);
        return new BadRequestObjectResult("Unsupported HTTP verb.");
    }

    private static string NormalizeBase64(string value)
    {
        var data = value.Trim();
        var commaIndex = data.IndexOf(',');
        if (commaIndex >= 0)
        {
            data = data[(commaIndex + 1)..];
        }

        return data;
    }

    private class ContractUploadRequest
    {
        public string FileName { get; set; }
        public string Base64 { get; set; }
    }
}
