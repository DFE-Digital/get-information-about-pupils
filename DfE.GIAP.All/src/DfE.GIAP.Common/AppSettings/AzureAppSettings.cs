using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Common.AppSettings
{
    [ExcludeFromCodeCoverage]
    public class AzureAppSettings
    {
        //Blob Storage configuration
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string StorageContainerName { get; set; }

        //DfE Sign-in related configuration
        public string DsiAudience { get; set; }
        public string DsiAuthorisationUrl { get; set; }
        public string DsiRedirectUrlAfterSignout { get; set; }
        public string DsiClientId { get; set; }
        public string DsiClientSecret { get; set; }
        public string DsiApiClientSecret { get; set; }

        public string DsiMetadataAddress { get; set; }
        public string DsiServiceId { get; set; }

        //Common
        public int SessionTimeout { get; set; }
        public bool IsSessionIdStoredInCookie { get; set; }

        //Search
        public int MaximumNonUPNResults { get; set; }
        public int MaximumUPNsPerSearch { get; set; }
        public int MaximumULNsPerSearch { get; set; }
        public int MaximumNonULNResults { get; set; }
        public int NonUpnPPMyPupilListLimit { get; set; }
        public int NonUpnNPDMyPupilListLimit { get; set; }
        public int UpnPPMyPupilListLimit { get; set; }
        public int UpnNPDMyPupilListLimit { get; set; }

        //Azure Function Urls
        public string CreateOrUpdateUserProfileUrl { get; set; }
        public string DeleteNewsArticleUrl { get; set; }
        public string DownloadPupilsByUPNsCSVUrl { get; set; }
        public string DownloadPupilPremiumByUPNFforCSVUrl { get; set; }
        public string DownloadPrepreparedFilesUrl { get; set; }
        public string DownloadCommonTransferFileUrl { get; set; }
        public string DownloadSecurityReportByUpnUrl { get; set; }
        public string DownloadSecurityReportByUlnUrl { get; set; }
        public string DownloadSecurityReportLoginDetailsUrl { get; set; }
        public string DownloadSecurityReportDetailedSearchesUrl { get; set; }
        public string GetAcademiesURL { get; set; }
        public string GetLatestNewsStatusUrl { get; set; }
        public string SetLatestNewsStatusUrl { get; set; }
        public string GetUserProfileUrl { get; set; }
        public string LoggingEventUrl { get; set; }
        public string GetContentByIDUrl { get; set; }
        public string QueryNewsArticlesUrl { get; set; }
        public string QueryNewsArticleUrl { get; set; }

        public string UpdateNewsDocumentUrl { get; set; }
        public string UpdateNewsPropertyUrl { get; set; }

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
        public string MetaDataDownloadListDirectory { get; set; }
        public int DownloadOptionsCheckLimit { get; set; }

        // flag indicating that we should show the LA number
        public bool UseLAColumn { get; set; }

        //flags for using gender on the search screens
        public bool NpdUseGender { get; set; }
        public bool PpUseGender { get; set; }
        public bool FeUseGender { get; set; }
        public string FeatureFlagAppConfigUrl { get; set; }
    }
}
