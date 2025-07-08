using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ExceptionMessage { get; set; }

        public string? Stacktrace { get; set; }

        public bool ShowError { get; set; }
    }
}