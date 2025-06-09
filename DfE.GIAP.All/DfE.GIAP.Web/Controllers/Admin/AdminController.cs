using System;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Service.Download.SecurityReport;
using DfE.GIAP.Service.Security;
using DfE.GIAP.Web.ViewModels.Admin;
using DfE.GIAP.Web.ViewModels.Admin.SecurityReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.SecurityReports;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Common.Constants.Messages.Downloads;

namespace DfE.GIAP.Web.Controllers.Admin;

[Authorize(Policy = Policy.RequireAdminApproverAccess)]
public class AdminController : Controller
{
    private readonly ISecurityService _securityService;
    private readonly ISessionProvider _sessionProvider;
    private readonly IDownloadSecurityReportByUpnUlnService _downloadSecurityReportByUpnService;
    private readonly IDownloadSecurityReportLoginDetailsService _downloadSecurityReportLoginDetailsService;
    private readonly IDownloadSecurityReportDetailedSearchesService _downloadSecurityReportDetailedSearchesService;

    public AdminController(ISecurityService securityService,
                          ISessionProvider sessionProvider,
                          IDownloadSecurityReportByUpnUlnService downloadSecurityReportByUpnService,
                          IDownloadSecurityReportLoginDetailsService downloadSecurityReportLoginDetailsService,
                          IDownloadSecurityReportDetailedSearchesService downloadSecurityReportDetailedSearchesService)
    {
        _securityService = securityService ??
            throw new ArgumentNullException(nameof(securityService));
        _sessionProvider = sessionProvider ??
            throw new ArgumentNullException(nameof(sessionProvider));
        _downloadSecurityReportByUpnService = downloadSecurityReportByUpnService ??
            throw new ArgumentNullException(nameof(downloadSecurityReportByUpnService));
        _downloadSecurityReportLoginDetailsService = downloadSecurityReportLoginDetailsService ??
            throw new ArgumentNullException(nameof(downloadSecurityReportLoginDetailsService));
        _downloadSecurityReportDetailedSearchesService = downloadSecurityReportDetailedSearchesService ??
            throw new ArgumentNullException(nameof(downloadSecurityReportDetailedSearchesService));
    }

    public IActionResult Index()
    {
        return View("../Admin/Index", GetAdminViewModel());
    }

    [HttpPost]
    [Route(Routes.SecurityReports.AdminOptions)]
    public IActionResult AdminOptions(AdminViewModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedAdminOption))
        {
            ModelState.AddModelError("NoAdminSelection", CommonErrorMessages.NoAdminSelection);
            return View("../Admin/Index", GetAdminViewModel());
        }

        IActionResult redirectToActionResult = GetRedirectActionForOption(model.SelectedAdminOption);
        if (redirectToActionResult is null)
        {
            return View("../Admin/Index", GetAdminViewModel());
        }

        return redirectToActionResult;
    }

    private IActionResult GetRedirectActionForOption(string selectedOption)
    {
        return selectedOption switch
        {
            "ManageDocuments" => RedirectToAction("ManageDocuments", "ManageDocuments"),
            "DownloadSecurityReportsByPupilOrStudent" => RedirectToAction("SecurityReportsByUpnUln", "SecurityReportByPupilStudentRecord"),
            "DownloadSecurityReportsByOrganisation" => RedirectToAction("SecurityReportsForYourOrganisation", "Admin"),
            "DownloadSecurityReportsBySchool" => User.IsAdmin()
                                    ? RedirectToAction("SchoolCollegeDownloadOptions", "Admin")
                                    : RedirectToAction("SecurityReportsBySchool", "Admin"),
            _ => null,// no redirection
        };
    }

    [HttpGet]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolSchoolCollegeDownload)]
    public IActionResult SchoolCollegeDownloadOptions()
    {
        ClearSessionData(); // clear out old selections
        return View("../Admin/SecurityReports/SchoolCollegeDownloadOptions", GetAdminViewModel());
    }

    [HttpPost]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolSchoolCollegeDownload)]
    public IActionResult SchoolCollegeDownloadOptions(AdminViewModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedOrganisationOption))
        {
            ModelState.AddModelError("NoOrganisationSelection", CommonErrorMessages.NoOrganisationSelection);
            return View("../Admin/SecurityReports/SchoolCollegeDownloadOptions", GetAdminViewModel());
        }

        switch (model.SelectedOrganisationOption)
        {
            case "AcademyTrust":
            case "FurtherEducation":
            case "LocalAuthority":
                _sessionProvider.SetSessionValue(SessionKeys.SelectedOrganisationOption, model.SelectedOrganisationOption);
                return RedirectToAction("SecurityReportsBySchool", "Admin");
        }

        return View("../Admin/SecurityReports/SchoolCollegeDownloadOptions", GetAdminViewModel());
    }




    [HttpGet]
    [Route(Routes.SecurityReports.SecurityReportsBySchool)]
    public async Task<IActionResult> SecurityReportsBySchool()
    {
        SecurityReportsBySchoolViewModel model = new();
        UpdateModelFromSessionData(model);
        await AddOrganisationsToViewBag(model);

        if (User.IsAdmin())
        {
            return View("../Admin/SecurityReports/SecurityReportsBySchool", model);
        }

        // TODO: Can this be set from model directly?
        model.SelectedOrganisationOption = User switch
        {
            var u when u.IsEstablishmentWithFurtherEducation() => "FurtherEducation",
            var u when u.IsOrganisationLocalAuthority() => "LocalAuthority",
            var u when u.IsOrganisationSingleAcademyTrust() || u.IsOrganisationMultiAcademyTrust() => "AcademyTrust",
            _ => model.SelectedOrganisationOption // or null if preferred
        };

        return View("../Admin/SecurityReports/SecurityReportsBySchool", model);
    }

    [HttpPost]
    [Route(Routes.SecurityReports.SecurityReportsBySchool)]
    public async Task<IActionResult> SecurityReportsBySchool(SecurityReportsBySchoolViewModel model)
    {
        await AddOrganisationsToViewBag(model);
        model.SelectedReportType = model.SelectedReportType != null ? SecurityHelper.SanitizeText(model.SelectedReportType) : null;
        model.SelectedOrganisationCode = model.SelectedOrganisationCode != null ? SecurityHelper.SanitizeText(model.SelectedOrganisationCode) : null;

        if (string.IsNullOrEmpty(model.SelectedOrganisationCode))
        {
            ModelState.AddModelError("NoOrganisationSelected", SecurityReportsConstants.SecurityReportsNoOrganisationSelected);
        }

        if (ModelState.IsValid)
        {
            UpdateSessionDataFromModel(model);
            return RedirectToAction("SecurityReportsBySchoolEstablishmentSelection", "Admin");
        }
        return View("../Admin/SecurityReports/SecurityReportsBySchool", model);
    }




    [HttpGet]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolEstablishmentSelection)]
    public async Task<IActionResult> SecurityReportsBySchoolEstablishmentSelection()
    {
        var model = new SecurityReportsBySchoolViewModel();
        UpdateModelFromSessionData(model);

        model.ListOfSelectItems = await GetSelectItemsForEstablishmentsByOrganisationCode(model.SelectedOrganisationCodeID, model.SelectedOrganisationCodeDocType);

        return View("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", model);
    }

    [HttpPost]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolDownload)]
    public async Task<IActionResult> DownloadSecurityReportsBySchool(SecurityReportsBySchoolViewModel model)
    {
        model.SelectedReportType = model.SelectedReportType != null ? SecurityHelper.SanitizeText(model.SelectedReportType) : null;
        model.SelectedOrganisationCode = model.SelectedOrganisationCode != null ? SecurityHelper.SanitizeText(model.SelectedOrganisationCode) : null;
        model.SelectedEstablishmentCode = model.SelectedEstablishmentCode != null ? SecurityHelper.SanitizeText(model.SelectedEstablishmentCode) : null;

        UpdateSessionDataFromModel(model);

        if (ModelState.IsValid)
        {
            if (model.SelectedEstablishmentCode == null)
            {
                await AddOrganisationsToViewBag(model);

                if (model.SelectedOrganisationCode == null)
                {
                    ModelState.AddModelError("NoOrganisationSelected", SecurityReportsConstants.SecurityReportsNoOrganisationSelected);
                    return View("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", model);
                }
                else
                {
                    ModelState.AddModelError("NoEstablishmentSelected", SecurityReportsConstants.NoEstablishmentSelected);
                    ViewBag.SelectedReportType = model.SelectedReportType;
                    ViewBag.SelectedOrganisationCode = model.SelectedOrganisationCode;
                    model.ListOfSelectItems = await GetSelectItemsForEstablishmentsByOrganisationCode(model.SelectedOrganisationCodeID, model.SelectedOrganisationCodeDocType);
                    return View("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", model);
                }
            }

            model.ProcessDownload = true;
        }

        model.ListOfSelectItems = await GetSelectItemsForEstablishmentsByOrganisationCode(model.SelectedOrganisationCodeID, model.SelectedOrganisationCodeDocType);
        return View("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", model);
    }

    [HttpGet]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolGetDownload, Name = Routes.SecurityReports.Names.GetSecurityReportDownload)]
    public async Task<IActionResult> GetSecurityReport(string reportType, string estabCode, string estabType)
    {
        SecurityReportTypes type;

        if (Enum.TryParse(reportType, true, out type))
        {
            ReturnFile downloadFile;

            string estabCodeID = (estabCode != null && estabCode.Contains("|")) ? estabCode.Substring(0, estabCode.IndexOf('|')) : estabCode;

            if (await CheckAccessToSecurityReport(estabCodeID))
            {
                switch (type)
                {
                    case SecurityReportTypes.LoginDetails:
                        downloadFile = await _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(estabCodeID, SecurityReportSearchType.UniqueReferenceNumber, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
                        break;
                    case SecurityReportTypes.DetailedSearches:
                        downloadFile = await _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(
                            estabCodeID,
                            SecurityReportSearchType.UniqueReferenceNumber,
                            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                            estabType.Equals("FurtherEducation")).ConfigureAwait(false);
                        break;
                    default:
                        downloadFile = new ReturnFile();
                        break;
                }
            }
            else downloadFile = new ReturnFile(); // TODO: What do we want here?

            // TODO: Why are we setting these in session?
            if (downloadFile.Bytes != null)
            {
                _sessionProvider.SetSessionValue(SessionKeys.NoSecurityReportContent, false.ToString());
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }
            else
            {
                _sessionProvider.SetSessionValue(SessionKeys.NoSecurityReportContent, true.ToString());
            }
        }

        return new NoContentResult();
    }

    [HttpGet]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolConfirmation, Name = Routes.SecurityReports.Names.SRConfirmation)]
    public IActionResult SecurityReportsBySchoolConfirmation()
    {
        var model = new SecurityReportsBySchoolViewModel();
        UpdateModelFromSessionData(model);

        return View("../Admin/SecurityReports/SecurityReportsBySchoolConfirmation", model);
    }

    [HttpPost]
    [Route(Routes.SecurityReports.SecurityReportsBySchoolConfirmation)]
    public IActionResult SecurityReportsBySchoolConfirmation(SecurityReportsBySchoolViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedConfirmationOption))
        {
            UpdateSessionDataFromModel(model);

            switch (model.SelectedConfirmationOption)
            {
                case "AnotherReport": return RedirectToAction("SecurityReportsBySchoolEstablishmentSelection", "Admin");
                case "ChangeReport": ClearSessionData(); return RedirectToAction("SecurityReportsBySchool", "Admin");
                case "Admin": return RedirectToAction("Index", "Admin");
            }
        }
        else
        {
            ModelState.AddModelError("NoConfirmationSelection", CommonErrorMessages.NoConfirmationSelection);
        }
        return View("../Admin/SecurityReports/SecurityReportsBySchoolConfirmation", model);
    }

    [HttpGet(Routes.SecurityReports.SecurityReportsByOrganisation)]
    public IActionResult SecurityReportsForYourOrganisation()
    {
        SecurityReportsForYourOrganisationModel model = new();
        PopulateSecurityReportsDropdown(model);

        return View("../Admin/SecurityReports/SecurityReportsForYourOrganisation", model);
    }

    [HttpPost(Routes.SecurityReports.SecurityReportsByOrganisation)]
    public async Task<IActionResult> SecurityReportsForYourOrganisation(SecurityReportsForYourOrganisationModel model)
    {
        if (string.IsNullOrWhiteSpace(model.DocumentId))
        {
            model.HasInvalidDocumentList = true;
            model.ErrorDetails = CommonErrorMessages.AdminSecurityReportRequired;
            ModelState.AddModelError("NoOrganisationalReportSelected", CommonErrorMessages.AdminSecurityReportRequired);

            PopulateSecurityReportsDropdown(model);
            return View("../Admin/SecurityReports/SecurityReportsForYourOrganisation", model);
        }

        model.DocumentId = SecurityHelper.SanitizeText(model.DocumentId);

        if (!Enum.TryParse<SecurityReportTypes>(model.DocumentId, true, out var reportType))
        {
            ModelState.AddModelError("InvalidSecurityReportType", "Invalid report type selected.");
            PopulateSecurityReportsDropdown(model);
            return View("../Admin/SecurityReports/SecurityReportsForYourOrganisation", model);
        }

        ReturnFile downloadFile = await GetReportFileAsync(reportType);
        if (downloadFile?.Bytes is not null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        model.ErrorDetails = DownloadErrorMessages.NoDataForOrganisationDownload;
        ModelState.AddModelError("NoDataForOrganisationalDownloadExists", model.ErrorDetails);
        PopulateSecurityReportsDropdown(model);

        return View("../Admin/SecurityReports/SecurityReportsForYourOrganisation", model);
    }


    // TODO: This is causing performance issues on security screens
    private async Task AddOrganisationsToViewBag(SecurityReportsBySchoolViewModel model)
    {
        var laNumber = User.GetLocalAuthorityNumberForLocalAuthority();
        var academyTrustUniqueIdentifier = User.GetUniqueIdentifier();

        if (User.IsAdmin() && laNumber == SecurityReportsConstants.DfeUserLocalAuthorityCode)
        {
            switch (model.SelectedOrganisationOption)
            {
                case "AcademyTrust":
                    model.ListOfSelectItems = await GetAcademyTrusts();
                    break;
                case "FurtherEducation":
                    model.ListOfSelectItems = await GetFurtherEducationOrganisations();
                    break;
                case "LocalAuthority":
                    model.ListOfSelectItems = await GetLocalAuthorities();
                    break;
            }

        }
        else if (User.IsOrganisationLocalAuthority())
        {
            laNumber = laNumber + "|LA";
            var localAuthorities = await GetLocalAuthorities(); // can we be more specific?
            model.ListOfSelectItems = localAuthorities.Where(x => x.Value == laNumber);
            model.SelectedOrganisationCode = laNumber;
        }
        else if (User.IsOrganisationMultiAcademyTrust() || User.IsOrganisationSingleAcademyTrust())
        {
            var docTypes = new List<string>();
            model.ListOfSelectItems = await GetAcademyTrusts(academyTrustUniqueIdentifier);
            /*Append MAT or SAT identifier for Cosmos partition query*/
            academyTrustUniqueIdentifier = academyTrustUniqueIdentifier + "|" + (User.IsOrganisationMultiAcademyTrust() ? "MAT" : "SAT");
            model.SelectedOrganisationCode = academyTrustUniqueIdentifier;

            var establishments = await GetSelectItemsForEstablishmentsByOrganisationCode(model.SelectedOrganisationCodeID, User.IsOrganisationMultiAcademyTrust() ? "MAT" : "SAT");
            model.ListOfSelectItems = establishments;
            ViewBag.SelectedAcademyTrustCode = academyTrustUniqueIdentifier;

            // SAT can have the only school they have already selected..
            if (User.IsOrganisationSingleAcademyTrust())
            {
                model.SelectedEstablishmentCode = establishments.FirstOrDefault().Value;
            }
        }
        else if (User.IsOrganisationFurtherEducation() || User.IsOrganisationEstablishmentWithFurtherEducation())
        {
            var feNumber = laNumber + "|FE";
            var furtherEducationOrganisations = await GetFurtherEducationOrganisations();
            model.ListOfSelectItems = furtherEducationOrganisations.Where(x => x.Value == feNumber);
            model.SelectedOrganisationCode = feNumber;
        }
        else if (User.IsOrganisationEstablishment())
        {
            var localAuthorityNumber = User.GetLocalAuthorityNumberForEstablishment();
            var establishmentUniqueReferenceNumber = User.GetUniqueReferenceNumber();
            var establishments = await GetSelectItemsForEstablishmentsByOrganisationCode(localAuthorityNumber, "LA");

            model.ListOfSelectItems = establishments.Where(x => x.Value == establishmentUniqueReferenceNumber);
            ViewBag.SelectedOrganisationCode = localAuthorityNumber;
            model.SelectedOrganisationCode = localAuthorityNumber;
            model.SelectedEstablishmentCode = establishmentUniqueReferenceNumber;
        }
    }

    // TODO: We have RBAC related concerns and multiple calls for data we should already have here1
    private async Task<bool> CheckAccessToSecurityReport(string estabCodeID)
    {
        if (User.IsAdmin()) return true;

        if (User.IsOrganisationMultiAcademyTrust() || User.IsOrganisationSingleAcademyTrust())
        {
            var establishments = await GetEstablishmentsByOrganisationCode(User.GetUniqueIdentifier(), User.IsOrganisationMultiAcademyTrust() ? "MAT" : "SAT");
            return establishments.Any(e => e.URN.Equals(estabCodeID));
        }

        if (User.IsOrganisationLocalAuthority())
        {
            var laNumber = User.GetLocalAuthorityNumberForLocalAuthority();
            var establishments = await GetEstablishmentsByOrganisationCode(laNumber, "LA");
            return establishments.Any(e => e.URN.Equals(estabCodeID));
        }

        if (User.IsOrganisationFurtherEducation() || User.IsOrganisationEstablishmentWithFurtherEducation())
        {
            var laNumber = User.GetLocalAuthorityNumberForLocalAuthority();
            var establishments = await GetEstablishmentsByOrganisationCode(laNumber, "FE");
            return establishments.Any(e => e.URN.Equals(estabCodeID));
        }

        if (User.IsOrganisationEstablishment())
        {
            return User.GetUniqueReferenceNumber().Equals(estabCodeID);
        }

        return false;
    }

    // TODO: Check how this model is being used in the views, do we really need this?
    private AdminViewModel GetAdminViewModel()
    {
        return new AdminViewModel
        {
            IsAdmin = User.IsAdmin(),
            IsApprover = User.IsApprover(),
            IsDepartmentUser = User.IsCurrentDepartmentUser(),
            IsOrganisationEstablishment = User.IsOrganisationEstablishment(),
            IsOrganisationEstablishmentWithFurtherEducation = User.IsOrganisationEstablishmentWithFurtherEducation()
        };
    }

    // TODO: Look to move these checks/calls to a service layer
    private async Task<List<SelectListItem>> GetLocalAuthorities()
    {
        var localAuthorityList = await _securityService.GetAllLocalAuthorities();
        return localAuthorityList.Select(x => new SelectListItem()
        {
            Value = x.Code + "|LA",
            Text = x.Description
        }).OrderBy(x => x.Text).ToList();
    }

    private async Task<List<SelectListItem>> GetAcademyTrusts(string id = null)
    {
        var academyTrustList = await _securityService.GetAcademyTrusts(User.GetAcademyListForUser(), id);

        return academyTrustList.Select(x => new SelectListItem()
        {
            Value = x.Code + "|" + x.DocType,
            Text = x.Description
        }).OrderBy(x => x.Text).ToList();
    }

    private async Task<List<SelectListItem>> GetFurtherEducationOrganisations()
    {
        var furtherEducationList = await _securityService.GetAllFurtherEducationOrganisations();

        return furtherEducationList.Select(x => new SelectListItem()
        {
            Value = x.Code + "|FE",
            Text = x.Description
        }).OrderBy(x => x.Text).ToList();
    }

    private async Task<List<SelectListItem>> GetSelectItemsForEstablishmentsByOrganisationCode(string id, string docType)
    {
        var establishmentList = await GetEstablishmentsByOrganisationCode(id, docType);

        return establishmentList.Select(x => new SelectListItem()
        {
            Value = x.URN + "|" + x.Name,
            Text = x.Description
        }).OrderBy(x => x.Text).ToList();
    }

    private async Task<IList<Establishment>> GetEstablishmentsByOrganisationCode(string id, string docType)
    {
        return docType == "LA" || docType == "FE" ?
            await _securityService.GetEstablishmentsByOrganisationCode(docType, id) :
            await _securityService.GetEstablishmentsByAcademyTrustCode(new List<string>() { docType }, id);
    }


    // Temp helper methods
    private void UpdateSessionDataFromModel(SecurityReportsBySchoolViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedReportType))
            _sessionProvider.SetSessionValue(SessionKeys.SelectedReportType, model.SelectedReportType);
        if (!string.IsNullOrEmpty(model.SelectedOrganisationCode))
            _sessionProvider.SetSessionValue(SessionKeys.SelectedOrganisationCode, model.SelectedOrganisationCode);
        if (!string.IsNullOrEmpty(model.SelectedEstablishmentCode))
            _sessionProvider.SetSessionValue(SessionKeys.SelectedEstablishmentCode, model.SelectedEstablishmentCode);
        if (!string.IsNullOrEmpty(model.SelectedOrganisationOption))
            _sessionProvider.SetSessionValue(SessionKeys.SelectedOrganisationOption, model.SelectedOrganisationOption);
    }

    private void UpdateModelFromSessionData(SecurityReportsBySchoolViewModel model)
    {
        if (_sessionProvider.ContainsSessionKey(SessionKeys.SelectedReportType))
            model.SelectedReportType = _sessionProvider.GetSessionValue(SessionKeys.SelectedReportType);
        if (_sessionProvider.ContainsSessionKey(SessionKeys.SelectedOrganisationCode))
            model.SelectedOrganisationCode = _sessionProvider.GetSessionValue(SessionKeys.SelectedOrganisationCode);
        if (_sessionProvider.ContainsSessionKey(SessionKeys.SelectedEstablishmentCode))
            model.SelectedEstablishmentCode = _sessionProvider.GetSessionValue(SessionKeys.SelectedEstablishmentCode);
        if (_sessionProvider.ContainsSessionKey(SessionKeys.SelectedOrganisationOption))
            model.SelectedOrganisationOption = _sessionProvider.GetSessionValue(SessionKeys.SelectedOrganisationOption);
        if (_sessionProvider.ContainsSessionKey(SessionKeys.NoSecurityReportContent))
            model.NoSRContent = Convert.ToBoolean(_sessionProvider.GetSessionValue(SessionKeys.NoSecurityReportContent));
    }

    private void ClearSessionData()
    {
        _sessionProvider.RemoveSessionValue(SessionKeys.SelectedReportType);
        _sessionProvider.RemoveSessionValue(SessionKeys.SelectedOrganisationCode);
        _sessionProvider.RemoveSessionValue(SessionKeys.SelectedEstablishmentCode);
        _sessionProvider.RemoveSessionValue(SessionKeys.NoSecurityReportContent);
    }

    private void PopulateSecurityReportsDropdown(SecurityReportsForYourOrganisationModel model)
    {
        model.SecurityReportTypes = Enum
            .GetValues(typeof(SecurityReportTypes))
            .Cast<SecurityReportTypes>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(),
                Text = e.GetDescription()
            }).ToList();
    }

    private async Task<ReturnFile> GetReportFileAsync(SecurityReportTypes reportType)
    {
        var organisationId = User.GetOrganisationId();
        var header = AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId());

        return reportType switch
        {
            SecurityReportTypes.LoginDetails =>
                await _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(organisationId, SecurityReportSearchType.OrganisationGuid, header),
            SecurityReportTypes.DetailedSearches =>
                await _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(
                    organisationId, SecurityReportSearchType.OrganisationGuid, header, User.IsEstablishmentWithFurtherEducation()),
            _ => new ReturnFile()
        };
    }
}
