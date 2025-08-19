using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.PrePreparedDownload;

[ExcludeFromCodeCoverage]
public class PreparedDownloadsViewModel
{
    public List<PreparedFileViewModel> PreparedDownloadFiles = new();
}
