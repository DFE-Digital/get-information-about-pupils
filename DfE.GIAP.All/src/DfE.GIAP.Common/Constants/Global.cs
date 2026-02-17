namespace DfE.GIAP.Common.Constants;

public static class Global
{
    public const int YearMinLimit = 1972;
    public const string StatusTrue = "true";
    public const string StatusFalse = "false";
    public const string UserAccount = "someuser";
    public const string UserName = "someUsername";
    public const string ContactLinkURL = "https://form.education.gov.uk/en/AchieveForms/?form_uri=sandbox-publish://AF-Process-2b61dfcd-9296-4f6a-8a26-4671265cae67/AF-Stage-f3f5200e-e605-4a1b-ae6b-3536bc77305c/definition.json&redirectlink=%2Fen&cancelRedirectLink=%2Fen";
    public const string UpnMask = "*************";
    public const string EncodedSuffixMarker = "-GIAP";

    // NPD
    public const string LearnerNumberLabel = "UPN";
    public const string NPDLearnerNumberSearchController = "NationalPupilDatabaseLearnerNumberSearch";
    public const string NPDLearnerNumberSearchAction = "NationalPupilDatabase";
    public const string NPDLearnerTextSearchDatabaseName = "NonUpnDatabaseName";
    public const string NonUpnSearchView = "~/Views/Shared/LearnerText/Search.cshtml";
    public const string NPDTextSearchController = "NationalPupilDatabaseLearnerTextSearch";
    public const string NPDAction = "NationalPupilDatabase";
    public const string NPDNonUpnAction = "NonUpnNationalPupilDatabase";
    public const string NPDNonUpnSearchSessionKey = "SearchNonUPN_SearchText";
    public const string NPDNonUpnSearchFiltersSessionKey = "SearchNonUPN_SearchFilters";
    public const string NPDNonUpnSortDirectionSessionKey = "SearchNonUPN_SortDirection";
    public const string NPDNonUpnSortFieldSessionKey = "SearchNonUPN_SortField";
    public const string NPDDownloadConfirmationReturnAction = "DownloadFileConfirmationReturn";
    public const string NPDDownloadCancellationReturnAction = "DownloadCancellationReturn";
    public const string NPDNonUpnDownloadLinksView = "~/Views/Shared/LearnerText/_SearchPageDownloadLinks.cshtml";

    // Pupil Premium
    public const string PPNonUpnSearchSessionKey = "SearchPPNonUPN_SearchText";
    public const string PPNonUpnSearchFiltersSessionKey = "SearchPPNonUPN_SearchFilters";
    public const string PPNonUpnSortDirectionSessionKey = "SearchPPNonUPN_SortDirection";
    public const string PPNonUpnSortFieldSessionKey = "SearchPPNonUPN_SortField";
    public const string PPTextSearchController = "PupilPremiumLearnerTextSearch";
    public const string PPNonUpnAction = "NonUpnPupilPremiumDatabase";
    public const string PPLearnerTextSearchDatabaseName = "NonUpnDatabaseName";
    public const string PPLearnerNumberSearchController = "PupilPremiumLearnerNumberSearch";
    public const string PPDownloadConfirmationReturnAction = "DownloadFileConfirmationReturn";
    public const string PPDownloadCancellationReturnAction = "DownloadCancellationReturn";
    public const string PPNonUpnDownloadLinksView = "~/Views/Shared/LearnerText/_SearchPupilPremiumDownloadLinks.cshtml";

    // Further Education
    public const string FELearnerNumberLabel = "ULN";
    public const string FENonUlnSearchSessionKey = "SearchNonULN_SearchText";
    public const string FENonUlnSearchFiltersSessionKey = "SearchNonULN_SearchFilters";
    public const string FENonUlnSortDirectionSessionKey = "SearchNonULN_SortDirection";
    public const string FENonUlnSortFieldSessionKey = "SearchNonULN_SortField";
    public const string FELearnerNumberSearchController = "FELearnerNumber";
    public const string FELearnerNumberSearchAction = "PupilUlnSearch";
    public const string FELearnerTextSearchController = "FELearnerTextSearch";
    public const string FELearnerTextSearchAction = "FurtherEducationNonUlnSearch";
    public const string FENonUlnDownloadLinksView = "~/Views/Shared/LearnerText/_SearchFurtherEducationDownloadLinks.cshtml";

    // Number Search
    public const string SearchView = "~/Views/Shared/LearnerNumber/Search.cshtml";
    public const string LearnerNumberSearchBoxViewDownloads = "../Shared/LearnerNumber/_SearchBoxDownloads";
    public const string DownloadNPDOptionsView = "../Shared/LearnerNumber/DownloadOptions";

    // Text Search
    public const string MPLDownloadNPDOptionsView = "../MyPupilList/DownloadOptions";
    public const string NonLearnerNumberDownloadOptionsView = "../Shared/LearnerText/DownloadOptions";
}
