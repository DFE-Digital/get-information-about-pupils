using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Common.AppSettings;

[ExcludeFromCodeCoverage]
public class AzureAppSettings
{
    //Common
    public bool IsSessionIdStoredInCookie { get; set; }

    //Search
    public int MaximumNonUPNResults { get; set; }
    public int MaximumUPNsPerSearch { get; set; }
    public int MaximumULNsPerSearch { get; set; }
    public int NonUpnPPMyPupilListLimit { get; set; }
    public int NonUpnNPDMyPupilListLimit { get; set; }
    public int UpnPPMyPupilListLimit { get; set; }
    public int UpnNPDMyPupilListLimit { get; set; }

    //Azure Function Urls
    public string CreateOrUpdateUserProfileUrl { get; set; }
    public string DownloadPupilsByUPNsCSVUrl { get; set; }
    public string DownloadPupilPremiumByUPNFforCSVUrl { get; set; }
    public string DownloadCommonTransferFileUrl { get; set; }
    public string DownloadSecurityReportByUpnUrl { get; set; }
    public string DownloadSecurityReportByUlnUrl { get; set; }
    public string DownloadSecurityReportLoginDetailsUrl { get; set; }
    public string DownloadSecurityReportDetailedSearchesUrl { get; set; }
    public string GetAcademiesURL { get; set; }
    public string GetUserProfileUrl { get; set; }
    public string LoggingEventUrl { get; set; }

    //Security reports
    public string QueryLAGetAllUrl { get; set; }
    public string QueryLAByCodeUrl { get; set; }
    public string GetAllFurtherEducationURL { get; set; }
    public string GetFurtherEducationByCodeURL { get; set; }

    //Further Education
    public string DownloadPupilsByULNsUrl { get; set; }

    // Paginated Search
    public string PaginatedSearchUrl { get; set; }

    //Downloads
    public int CommonTransferFileUPNLimit { get; set; }
    public int DownloadOptionsCheckLimit { get; set; }

    // flag indicating that we should show the LA number
    public bool UseLAColumn { get; set; }
}
