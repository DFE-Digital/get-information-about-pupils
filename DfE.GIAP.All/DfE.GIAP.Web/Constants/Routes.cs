namespace DfE.GIAP.Web.Constants;
public static class Routes
{
    public const string DownloadSelectedNationalPupilDatabaseData = "download";
    public const string NPDNonUpnAddToMyPupilList = "add-npd-nonupn-to-my-pupil-list";
    public const string NPDInvalidUPNs = "npd-invalid-upns";
    public const string NPDInvalidUPNsConfirmation = "npd-invalid-upns-confirmation";
    public const string NPDNonUpnInvalidUPNs = "npd-nonupn-invalid-upns";
    public const string NPDNonUpnInvalidUPNsConfirmation = "npd-nonupn-invalid-upns-confirmation";
    public const string PPInvalidUPNs = "pp-invalid-upns";
    public const string PPInvalidUPNsConfirmation = "pp-invalid-upns-confirmation";
    public const string PPNonUpnInvalidUPNs = "pp-nonupn-invalid-upns";
    public const string PPNonUpnInvalidUPNsConfirmation = "pp-nonupn-invalid-upns-confirmation";
    public const string InvalidUPNsReturn = "InvalidUPNsReturn";

    public static class Application
    {
        public const string Admin = "admin";
        public const string Consent = "consent";
        public const string Error = "error";
        public const string Home = "home";
        public const string News = "news";
        public const string UserWithNoRole = "user-with-no-role";
        public const string MyPupilList = "my-pupil-list";
        public const string PupilPremium = "pupil-premium";
        public const string Landing = "landing";
        public const string Search = "search";
        public const string SimulateError = "simulate-error";
    }

    public static class Authentication
    {
        public const string AuthenticationController = "auth";
        public const string LoginAction = "login";
        public const string SignoutAction = "signout";
    }

    public static class DownloadCommonTransferFile
    {
        public const string DownloadCommonTransferFileAction = "download-ctf";
        public const string DownloadCTFNoDataReturn = "DownloadCTFNoDataReturn";
        public const string RedirectDownloadCTF = "download-ctf";
        public const string DownloadCommonTransferFileDataNonUPN = "download-ctf-non-upn";
        public const string DownloadNpdNonUpnCommonTransferFileAction = "npd-non-upn-download-ctf";

    }

    public static class ManageDocument
    {
        public const string ManageDocuments = "manage-documents/{docType?}/{docAction?}/{newsArticleId?}";
        public const string ManageDocumentsPreview = "manage-documents/preview";
        public const string ManageDocumentsPublish = "manage-documents/publish";
        public const string ManageDocumentsNewsArticleAdd = "manage-documents/article/add";
        public const string ManageDocumentsNewsArticleArchive = "manage-documents/article/archive";
        public const string ManageDocumentsNewsArticleArchived = "manage-documents/article/archived";
        public const string ManageDocumentsNewsArticleDelete = "manage-documents/article/delete";
        public const string ManageDocumentsArchivedNewsArticleDelete = "manage-documents/article/archived/delete";
        public const string ManageDocumentsNewsArticleEdit = "manage-documents/article/edit";
        public const string ManageDocumentsNewsArticlePreview = "manage-documents/article/preview";
        public const string ManageDocumentsNewsArticlePublish = "manage-documents/article/publish";
        public const string ManageDocumentsNewsArticleUnarchive = "manage-documents/article/unarchive";
        public const string ManageDocumentsNewsArticleSaveAsDraft = "manage-documents/article/save-as-draft";
    }

    public static class PrePreparedDownloads
    {
        public const string PreparedDownloadsController = "data-download";
        public const string DownloadPrePreparedFileAction = "data-download";
    }

    public static class SecurityReports
    {
        public const string SecurityReportsByUpnUln = "security-reports-by-upn-uln";
        public const string SecurityReportsBySchool = "admin/security-reports-by-school";
        public const string SecurityReportsBySchoolDownload = "admin/security-reports-by-school-download";
        public const string SecurityReportsBySchoolConfirmation = "admin/security-reports-by-school-confirmation";
        public const string SecurityReportsBySchoolGetDownload = "admin/security-reports-by-school-get-download/{reportType}/{estabCode}";
        public const string AdminOptions = "admin/admin-options";
        public const string SecurityReportsBySchoolSchoolCollegeDownload = "admin/school-college-download";
        public const string SecurityReportsBySchoolEstablishmentSelection = "admin/establishment-selection";
        public const string SecurityReportsByOrganisation = "admin/security-reports-for-your-organisation-school";
        public const string SecurityReportsByOrganisationDownload = "admin/security-reports-for-your-organisation-download";
        public const string SecurityReportsByUpnNoDataReturn = "SecurityReportsByUpnNoDataReturn";

        public static class Names
        {
            public const string GetSecurityReportDownload = "GetSRDownload";
            public const string SRConfirmation = "SRConfirmation";
        }
    }

    public static class MyPupilList
    {
        public const string AddToMyPupilList = "AddToMyPupilList";
        public const string RemoveSelected = "RemoveSelected";
        public const string DownloadNonUPNConfirmationReturn = "mpl-nonupn-starred-pupil-confirmation";
        public const string DownloadCancellationReturn = "mpl-starred-pupil-cancellation";
        public const string MyPupilListView = "~/Views/MyPupilList/Index.cshtml";
        public const string MyPupilListViewConfirmation = "~/Views/MyPupilList/PupilListConfirmation.cshtml";
    }

    public static class NationalPupilDatabase
    {
        public const string NationalPupilDatabaseLearnerNumber = "national-pupil-database";
        public const string NationalPupilDatabaseNonUPN = "npd-nonupn";
        public const string NationalPupilDatabaseLearnerNumberWithPage = "national-pupil-database/{pageNumber:int?}";
        public const string LearnerNumberDownloadFile = "download-file";
        public const string LearnerNumberDownloadRequest = "download-upn";
        public const string LearnerTextDataDownloadRequest = "download-non-upn";
        public const string LearnerTextDownloadOptions = "non-upn-download";
        public const string LearnerTextDownloadFile = "download-file-non-upn";
        public const string DownloadCTFData = "npd-non-upn-download-ctf";
        public const string DownloadNonUPNConfirmationReturn = "mpl-nonupn-starred-pupil-confirmation";
        public const string DownloadCancellationReturn = "mpl-starred-pupil-cancellation";
        public const string NonUpnDobFilter = "/search/npd-nonupn/dob-filter";
        public const string NonUpnGenderFilter = "/search/npd-nonupn/gender-filter";
        public const string NonUpnSexFilter = "/search/npd-nonupn/sex-filter";
        public const string NonUpnForenameFilter = "/search/npd-nonupn/forename-filter";
        public const string NonUpnMiddlenameFilter = "/search/npd-nonupn/middlename-filter";
        public const string NonUpnSurnameFilter = "/search/npd-nonupn/surname-filter";
        public const string NonUpnNationalPupilDatabaseReturn = "NonUpnNationalPupilDatabaseReturn";
    }

    public static class PupilPremium
    {
        public const string PupilPremiumDatabase = "pupil-premium";
        public const string NonUPN = "pp-nonupn";
        public const string NonUPNSurnameFilter = "/search/pp-nonupn/surname-filter";
        public const string NonUpnDobFilter = "/search/pp-nonupn/dob-filter";
        public const string NonUpnGenderFilter = "/search/pp-nonupn/gender-filter";
        public const string NonUpnSexFilter = "/search/pp-nonupn/sex-filter";
        public const string NonUpnForenameFilter = "/search/pp-nonupn/forename-filter";
        public const string NonUpnMiddlenameFilter = "/search/pp-nonupn/middlename-filter";
        public const string DownloadSelectedPupilPremiumDatabaseData = "download";
        public const string DownloadSelectedPupilPremiumNonUpnDatabaseData = "download-pupilpremium-nonpun";
        public const string LearnerNumberDownloadRequest = "download-pp-upn";
        public const string LearnerTextDownloadRequest = "download-pp-nonupn";
        public const string DownloadPupilPremiumFile = "pp-download-file";
        public const string NonUpnPupilPremiumDatabaseReturn = "NonUpnPupilPremiumDatabaseReturn";
        public const string DownloadNonUPNConfirmationReturn = "pp-nonupn-starred-pupil-confirmation";
        public const string DownloadCancellationReturn = "pp-nonupn-starred-pupil-cancellation";
    }

    public static class FurtherEducation
    {
        public const string LearnerNumberSearch = "pupil-uln";
        public const string LearnerTextSearch = "pupil-non-uln";
        public const string NonULNSurnameFilter = "/search/fe-non-uln/surname-filter";
        public const string NonULNForenameFilter = "/search/fe-non-uln/forename-filter";
        public const string NonULNDobFilter = "/search/fe-non-uln/dob-filter";
        public const string NonULNGenderFilter = "/search/fe-non-uln/gender-filter";
        public const string NonULNSexFilter = "/search/fe-non-uln/sex-filter";
        public const string DownloadNonUlnRequest = "download-nonuln-fe";
        public const string DownloadNonUlnFile = "download-nonuln-fe-file";
    }
}
