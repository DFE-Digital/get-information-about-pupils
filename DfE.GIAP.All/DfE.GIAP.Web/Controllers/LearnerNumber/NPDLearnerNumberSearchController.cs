using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.LearnerNumber;

[Route(Routes.Application.Search)]
public class NPDLearnerNumberSearchController : BaseLearnerNumberController
{
    private readonly ILogger<NPDLearnerNumberSearchController> _logger;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    public override string PageHeading => ApplicationLabel.SearchNPDWithUpnPageHeading;
    public override string SearchAction => "NationalPupilDatabase";
    public override string FullTextLearnerSearchController => "Search";
    public override string FullTextLearnerSearchAction => Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;
    public override string InvalidUPNsConfirmationAction => "NPDInvalidUPNsConfirmation";
    public override string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchPageDownloadLinks.cshtml";
    public override AzureSearchIndexType IndexType => AzureSearchIndexType.NPD;
    public override string SearchSessionKey => "SearchNPD_SearchText";
    public override string SearchSessionSortField => "SearchNPD_SearchTextSortField";
    public override string SearchSessionSortDirection => "SearchNPD_SearchTextSortDirection";
    public override int MyPupilListLimit => _appSettings.UpnNPDMyPupilListLimit;
    public override bool ShowLocalAuthority => _appSettings.UseLAColumn;
    public override bool ShowMiddleNames => true;
    public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedNationalPupilDatabaseDataLink;

    public override string LearnerNumberLabel => Global.LearnerNumberLabel;


    public NPDLearnerNumberSearchController(ILogger<NPDLearnerNumberSearchController> logger,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IPaginatedSearchService paginatedSearch,
        IMyPupilListService mplService,
        ISelectionManager selectionManager,
        IContentService contentService,
        IOptions<AzureAppSettings> azureAppSettings)
        : base(logger, paginatedSearch, mplService, selectionManager, contentService, azureAppSettings)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _ctfService = ctfService ??
            throw new ArgumentNullException(nameof(ctfService));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
        _appSettings = azureAppSettings.Value;
    }


    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber)]
    [HttpGet]
    public async Task<IActionResult> NationalPupilDatabase(bool? returnToSearch)
    {
        _logger.LogInformation("National pupil database Upn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber)]
    [HttpPost]
    public async Task<IActionResult> NationalPupilDatabase(
        [FromForm] LearnerNumberSearchViewModel model,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("National pupil database Upn POST method called");

        return await Search(
            model,
            pageNumber,
            sortField,
            sortDirection,
            !ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [HttpPost]
    [Route(Routes.NPDInvalidUPNs)]
    public async Task<IActionResult> NPDInvalidUPNs(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNs(model);
    }

    [HttpPost]
    [Route(Routes.NPDInvalidUPNsConfirmation)]
    public async Task<IActionResult> NPDInvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNsConfirmation(model);
    }


    [HttpPost]
    [Route("add-npd-to-my-pupil-list")]
    public async Task<IActionResult> NPDAddToMyPupilList(LearnerNumberSearchViewModel model)
    {
        return await AddToMyPupilList(model);
    }


    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [HttpPost]
    public async Task<IActionResult> DownloadCommonTransferFileData(LearnerNumberSearchViewModel model)
    {
        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        var available = model.LearnerNumberIds.FormatLearnerNumbers();
        var selected = GetSelected(available);

        if (selected.Count == 0)
        {
            model.ErrorDetails = DownloadErrorMessages.NoPupilSelected;
            model.NoPupil = true;
            model.NoPupilSelected = true;
            return await NationalPupilDatabase(model, model.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        if (selected.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            model.ErrorDetails = DownloadErrorMessages.UPNLimitExceeded;
            return await NationalPupilDatabase(model, model.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        var downloadFile = await _ctfService.GetCommonTransferFile(selected.ToArray(),
                                                                model.LearnerNumber.FormatLearnerNumbers(),
                                                                User.GetLocalAuthorityNumberForEstablishment(),
                                                                User.GetEstablishmentNumber(),
                                                                User.IsOrganisationEstablishment(),
                                                                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                                                                ReturnRoute.NationalPupilDatabase);

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }
        else
        {
            model.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
        }

        return await NationalPupilDatabase(model, model.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
    }

    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
        string selectedPupilsJoined,
        string learnerNumber,
        int selectedPupilsCount)
    {
        var searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupilsJoined,
            LearnerNumber = learnerNumber,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        SearchDownloadHelper.AddDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationLowAge(), User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(), User.IsOrganisationAllAges());

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        ModelState.Clear();

        PopulateNavigation(searchDownloadViewModel.NumberSearchViewModel);
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = searchDownloadViewModel.LearnerNumber;
        searchDownloadViewModel.SearchAction = Global.NPDAction;
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumberLabel = "UPN";

        var selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForNoDataAvailable(selectedPupils, learnerNumber.FormatLearnerNumbers(), downloadTypeArray, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }
        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.DownloadNPDOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!String.IsNullOrEmpty(model.SelectedPupils))
        {
            var selectedPupils = model.SelectedPupils.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = SearchErrorMessages.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = model.DownloadFileType == DownloadFileType.CSV ?
                    await _downloadService.GetCSVFile(selectedPupils, model.LearnerNumber.FormatLearnerNumbers(), model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase).ConfigureAwait(false) :
                    await _downloadService.GetTABFile(selectedPupils, model.LearnerNumber.FormatLearnerNumbers(), model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase).ConfigureAwait(false);

                if (downloadFile == null)
                {
                    return RedirectToAction(Routes.Application.Error, Routes.Application.Home);
                }

                if (downloadFile.Bytes != null)
                {
                    model.ErrorDetails = null;

                    return SearchDownloadHelper.DownloadFile(downloadFile);
                }
                else
                {
                    model.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
                }
            }
            else
            {
                model.ErrorDetails = SearchErrorMessages.SelectFileType;
            }

            TempData["ErrorDetails"] = model.ErrorDetails;

            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }

        return RedirectToAction(Global.NPDLearnerNumberSearchAction, Global.NPDLearnerNumberSearchController);
    }

    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedNPDDataUPN(LearnerNumberSearchViewModel searchViewModel)
    {
        SetSelections(
            searchViewModel.PageLearnerNumbers.Split(','),
            searchViewModel.SelectedPupil);

        var selectedPupils = GetSelected(searchViewModel.LearnerNumberIds.FormatLearnerNumbers());

        if (selectedPupils.Count == 0)
        {
            searchViewModel.NoPupil = true;
            searchViewModel.NoPupilSelected = true;
            return await NationalPupilDatabase(searchViewModel, searchViewModel.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        var joinedSelectedPupils = String.Join(',', selectedPupils);
        return await DownloadSelectedNationalPupilDatabaseData(joinedSelectedPupils, searchViewModel.LearnerNumber, selectedPupils.Count);
    }

    protected override async Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model)
    {
        return await NationalPupilDatabase(model, model.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
    }

    protected override bool ValidateLearnerNumber(string learnerNumber)
    {
        return ValidationHelper.IsValidUpn(learnerNumber);
    }

    protected override string GenerateValidationMessage()
    {
        return PupilHelper.GenerateValidationMessageUpnSearch(ModelState);
    }
}
