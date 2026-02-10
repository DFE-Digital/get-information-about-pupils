using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.PreparedDownload;

[ExcludeFromCodeCoverage]
public class PreparedDownloadsViewModel
{
    public List<PreparedFileViewModel> PreparedDownloadFiles = new();
}
