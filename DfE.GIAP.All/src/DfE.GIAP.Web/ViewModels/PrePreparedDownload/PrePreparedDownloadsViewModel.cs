using DfE.GIAP.Domain.Models.PrePreparedDownloads;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.PrePreparedDownload;

[ExcludeFromCodeCoverage]
public class PrePreparedDownloadsViewModel
{
    public List<PrePreparedFileViewModel> PrePreparedDownloadList = new();
}
