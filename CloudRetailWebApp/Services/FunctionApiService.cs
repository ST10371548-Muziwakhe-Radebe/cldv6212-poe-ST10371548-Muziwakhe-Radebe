using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;



// PURPOSE: Helper for calling Azure Function endpoints from the MVC app.
// FOUND AT: https://learn.microsoft.com/dotnet/api/system.net.http.httpclient


namespace CloudRetailWebApp.Services
{
    public class FunctionApiService
    {
        private readonly HttpClient _httpClient;

        public FunctionApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Example method to post JSON to a Function
        public async Task<bool> PostToFunctionAsync(string functionUrl, object payload)
        {
            var response = await _httpClient.PostAsJsonAsync(functionUrl, payload);
            return response.IsSuccessStatusCode;
        }
    }
}
