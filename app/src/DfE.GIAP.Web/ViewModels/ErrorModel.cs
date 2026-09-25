using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? Stacktrace { get; set; }
    public bool ShowError { get; set; }
}
