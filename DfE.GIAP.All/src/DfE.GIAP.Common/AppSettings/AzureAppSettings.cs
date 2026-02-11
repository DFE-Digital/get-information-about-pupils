using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Common.AppSettings;

[ExcludeFromCodeCoverage]
public class AzureAppSettings
{
    //Common
    public bool IsSessionIdStoredInCookie { get; set; }

    //Downloads
    public string DownloadCommonTransferFileUrl { get; set; }
    public int CommonTransferFileUPNLimit { get; set; }
    public int DownloadOptionsCheckLimit { get; set; }

}
