using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CloudRetailWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

// PURPOSE: Helper for calling Azure Function endpoints from the MVC app.
// FOUND AT: https://learn.microsoft.com/dotnet/api/system.net.http.httpclient

namespace CloudRetailWebApp.Services
{
    public class FunctionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionKey;
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

        public FunctionApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _functionKey = configuration["Azure:FunctionKey"] ?? string.Empty;

            if (_httpClient.BaseAddress == null)
            {
                var baseUrl = configuration["Azure:FunctionBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl) || baseUrl.Contains("YOUR_FUNCTION_APP_NAME"))
                {
                    throw new InvalidOperationException(
                        "Azure:FunctionBaseUrl is not configured in appsettings.json. " +
                        "Please set it to your Azure Function App URL (e.g., https://your-function-app.azurewebsites.net/api/)");
                }
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        // Example method to post JSON to a Function
        public async Task<bool> PostToFunctionAsync(string functionUrl, object payload)
        {
            var response = await _httpClient.PostAsJsonAsync(functionUrl, payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<IReadOnlyList<OrderMessageModel>> GetQueueMessagesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BuildUrl("orders/queue"));
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<OrderMessageModel>();
                }

                var data = await response.Content.ReadFromJsonAsync<List<OrderMessageModel>>(_serializerOptions);
                return data ?? new List<OrderMessageModel>();
            }
            catch
            {
                return Array.Empty<OrderMessageModel>();
            }
        }

        public async Task<IReadOnlyList<string>> GetContractFilesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BuildUrl("contracts"));
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<string>();
                }

                var files = await response.Content.ReadFromJsonAsync<List<string>>(_serializerOptions);
                return files ?? new List<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public async Task<byte[]?> DownloadContractAsync(string fileName)
        {
            try
            {
                var response = await _httpClient.GetAsync(BuildUrl($"contracts/{Uri.EscapeDataString(fileName)}"));
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UploadContractAsync(IFormFile file)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                content.Add(streamContent, "file", file.FileName);

                var response = await _httpClient.PostAsync(BuildUrl("contracts"), content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteContractAsync(string fileName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(BuildUrl($"contracts/{Uri.EscapeDataString(fileName)}"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private string BuildUrl(string relativePath)
        {
            if (_httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress is not configured. Check Azure:FunctionBaseUrl in appsettings.json.");
            }
            var absolute = new Uri(_httpClient.BaseAddress, relativePath.TrimStart('/'));
            if (string.IsNullOrWhiteSpace(_functionKey))
            {
                return absolute.ToString();
            }

            return QueryHelpers.AddQueryString(absolute.ToString(), "code", _functionKey);
        }
    }
}
