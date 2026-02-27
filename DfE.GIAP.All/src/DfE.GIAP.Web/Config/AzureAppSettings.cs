using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Config;

[ExcludeFromCodeCoverage]
public class AzureAppSettings
{
    //Downloads
    public int CommonTransferFileUPNLimit { get; set; }
    public int DownloadOptionsCheckLimit { get; set; }
}
