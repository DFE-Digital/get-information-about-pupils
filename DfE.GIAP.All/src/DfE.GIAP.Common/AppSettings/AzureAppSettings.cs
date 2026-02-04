using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Common.AppSettings;

[ExcludeFromCodeCoverage]
public class AzureAppSettings
{
    //Common
    public bool IsSessionIdStoredInCookie { get; set; }

    public string DownloadPupilsByUPNsCSVUrl { get; set; }
    public string DownloadPupilPremiumByUPNFforCSVUrl { get; set; }
    public string DownloadCommonTransferFileUrl { get; set; }

    //Further Education
    public string DownloadPupilsByULNsUrl { get; set; }

    //Downloads
    public int CommonTransferFileUPNLimit { get; set; }
    public int DownloadOptionsCheckLimit { get; set; }

}
