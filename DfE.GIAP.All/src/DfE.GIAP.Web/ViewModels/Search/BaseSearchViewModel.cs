using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Search;

[ExcludeFromCodeCoverage]
public class BaseSearchViewModel
{
    public string ErrorDetails { get; set; }
    public string SearchAction { get; set; }
    public string SearchResultPageHeading { get; set; }
    public string DownloadRoute { get; set; }
    public string RedirectRoute { get; set; }
}
