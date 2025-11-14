// DESCRIPTION: Defines the ErrorViewModel used for passing error request details
//              to the view in CloudRetailWebApp, including a request identifier.
// SOURCES:
//    - ASP.NET Core MVC Error Handling: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
//    - C# String.IsNullOrEmpty Method: https://learn.microsoft.com/en-us/dotnet/api/system.string.isnullorempty

namespace CloudRetailWebApp.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
