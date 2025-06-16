using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
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

namespace DfE.GIAP.Web.Controllers.TextBasedSearch;

[Route(Routes.Application.Search)]
public class NPDLearnerTextSearchController : BaseLearnerTextSearchController
{
    private readonly ILogger<NPDLearnerTextSearchController> _logger;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    public override string PageHeading => ApplicationLabel.SearchNPDWithOutUpnPageHeading;
    public override string SearchSessionKey => Global.NPDNonUpnSearchSessionKey;
    public override string SearchFiltersSessionKey => Global.NPDNonUpnSearchFiltersSessionKey;
    public override string DownloadLinksPartial => Global.NPDNonUpnDownloadLinksView;

    public override string SortDirectionKey => Global.NPDNonUpnSortDirectionSessionKey;
    public override string SortFieldKey => Global.NPDNonUpnSortFieldSessionKey;

    public override string SearchLearnerNumberAction => Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber;
    public override string RedirectUrlFormAction => Global.NPDNonUpnAction;
    public override string LearnerTextDatabaseAction => Global.NPDNonUpnAction;
    public override string LearnerTextDatabaseName => Global.NPDLearnerTextSearchDatabaseName;
    public override string RedirectFrom => Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;


    public override string SurnameFilterUrl => Routes.NationalPupilDatabase.NonUpnSurnameFilter;
    public override string DobFilterUrl => Routes.NationalPupilDatabase.NonUpnDobFilter;
    public override string ForenameFilterUrl => Routes.NationalPupilDatabase.NonUpnForenameFilter;
    public override string MiddlenameFilterUrl => Routes.NationalPupilDatabase.NonUpnMiddlenameFilter;
    public override string GenderFilterUrl => Routes.NationalPupilDatabase.NonUpnGenderFilter;
    public override string SexFilterUrl => Routes.NationalPupilDatabase.NonUpnSexFilter;
    public override string FormAction => Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;
    public override string RemoveActionUrl => $"/{Routes.Application.Search}/{Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN}";
    public override AzureSearchIndexType IndexType => AzureSearchIndexType.NPD;
    public override string SearchView => Global.NonUpnSearchView;


    public override string SearchLearnerNumberController => Routes.Application.Search;

    public override string SearchAction => Global.NPDNonUpnAction;
    public override string SearchController => Global.NPDTextSearchController;
    public override int MyPupilListLimit => _appSettings.NonUpnNPDMyPupilListLimit;
    public override ReturnRoute ReturnRoute => Common.Enums.ReturnRoute.NonNationalPupilDatabase;
    public override string LearnerTextSearchController => Global.NPDTextSearchController;
    public override string LearnerTextSearchAction => SearchAction;
    public override string LearnerNumberAction => Global.NPDAction;
    public override bool ShowGender => _appSettings.NpdUseGender;
    public override bool ShowLocalAuthority => _appSettings.UseLAColumn;
    public override string InvalidUPNsConfirmationAction => Global.NPDNonUpnInvalidUPNsConfirmation;
    public override string LearnerNumberLabel => Global.LearnerNumberLabel;
    public override bool ShowMiddleNames => true;
    public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedNationalPupilDatabaseDataLink;



    public NPDLearnerTextSearchController(ILogger<NPDLearnerTextSearchController> logger,
       IPaginatedSearchService paginatedSearch,
       IMyPupilListService mplService,
       ITextSearchSelectionManager selectionManager,
       IContentService contentService,
       IDownloadCommonTransferFileService ctfService,
       IDownloadService downloadService,
       IOptions<AzureAppSettings> azureAppSettings)
       : base(logger,
             paginatedSearch,
             mplService,
             selectionManager,
             contentService,
             azureAppSettings)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _ctfService = ctfService ??
            throw new ArgumentNullException(nameof(ctfService));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
        _appSettings = azureAppSettings.Value;
    }



    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN)]
    [HttpGet]
    public async Task<IActionResult> NonUpnNationalPupilDatabase(bool? returnToSearch)
    {
        _logger.LogInformation("National pupil database NonUpn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN)]
    [HttpPost]
    public async Task<IActionResult> NonUpnNationalPupilDatabase(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("National pupil database NonUpn POST method called");
        model.ShowHiddenUPNWarningMessage = true;
        var m = await Search(
            model, surnameFilter, middlenameFilter, forenameFilter,
            searchByRemove, model.PageNumber,
            ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController, sortField, sortDirection,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));

        return m;
    }



    [Route(Routes.NationalPupilDatabase.NonUpnDobFilter)]
    [HttpPost]
    public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
    {
        return await DobSearchFilter(model);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnSurnameFilter)]
    [HttpPost]
    public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
    {
        return await SurnameSearchFilter(model, surnameFilter);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnMiddlenameFilter)]
    [HttpPost]
    public async Task<IActionResult> MiddlenameFilter(LearnerTextSearchViewModel model, string middlenameFilter)
    {
        return await MiddlenameSearchFilter(model, middlenameFilter);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnForenameFilter)]
    [HttpPost]
    public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
    {
        return await ForenameSearchFilter(model, forenameFilter);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnGenderFilter)]
    [HttpPost]
    public async Task<IActionResult> GenderFilter(LearnerTextSearchViewModel model)
    {
        return await GenderSearchFilter(model);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        return await SexSearchFilter(model);
    }


    [HttpPost]
    [Route(Routes.NPDNonUpnAddToMyPupilList)]
    public async Task<IActionResult> NonUpnAddToMyPupilList(LearnerTextSearchViewModel model)
    {
        return await AddToMyPupilList(model);
    }


    [Route(Routes.NationalPupilDatabase.DownloadCTFData)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadNpdCommonTransferFileData(LearnerTextSearchViewModel model)
    {
        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        var selectedPupil = GetSelected();

        if (string.IsNullOrEmpty(selectedPupil))
        {
            model.ErrorDetails = DownloadErrorMessages.NoPupilSelected;
            model.NoPupil = true;
            model.NoPupilSelected = true;
            return await ReturnToSearch(model);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupil) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            PopulateConfirmationNavigation(model.StarredPupilConfirmationViewModel);
            model.StarredPupilConfirmationViewModel.DownloadType = DownloadType.CTF;
            model.StarredPupilConfirmationViewModel.SelectedPupil = selectedPupil;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        model.SelectedPupil = selectedPupil;

        return await DownloadNpdCommonTransferFileData(model);
    }

    private async Task<IActionResult> DownloadNpdCommonTransferFileData(LearnerTextSearchViewModel model)
    {
        var selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupil) ? RbacHelper.DecryptUpn(model.SelectedPupil) : model.SelectedPupil;

        var downloadFile = await _ctfService.GetCommonTransferFile(new string[] { selectedPupil },
                                                                new string[] { ValidationHelper.IsValidUpn(selectedPupil) ? selectedPupil : "0" },
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

        return await ReturnToSearch(model);
    }

    [Route(Routes.NationalPupilDatabase.DownloadNonUPNConfirmationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationError = !model.ConfirmationGiven;
        PopulateConfirmationNavigation(model);

        if (model.ConfirmationGiven)
        {
            switch (model.DownloadType)
            {
                case DownloadType.CTF: return await DownloadNpdCommonTransferFileData(new LearnerTextSearchViewModel() { SelectedPupil = model.SelectedPupil });
                case DownloadType.NPD: return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupil, this.HttpContext.Session.Keys.Contains(SearchSessionKey) ? this.HttpContext.Session.GetString(SearchSessionKey) : string.Empty);
            }
        }

        return ConfirmationForStarredPupil(model);
    }

    [Route(Routes.NationalPupilDatabase.DownloadCancellationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadCancellationReturn(StarredPupilConfirmationViewModel model)
    {
        return await Search(true);
    }

    private void PopulateConfirmationNavigation(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationReturnController = SearchController;
        model.ConfirmationReturnAction = Global.NPDDownloadConfirmationReturnAction;
        model.CancelReturnController = SearchController;
        model.CancelReturnAction = Global.NPDDownloadCancellationReturnAction;
    }

    [Route(Routes.NationalPupilDatabase.LearnerTextDataDownloadRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedNPDDataNonUPN(LearnerTextSearchViewModel model)
    {
        SetSelections(
        model.PageLearnerNumbers.Split(','),
        model.SelectedPupil);

        var selectedPupil = GetSelected();

        if (string.IsNullOrEmpty(selectedPupil))
        {
            model.ErrorDetails = DownloadErrorMessages.NoPupilSelected;
            model.NoPupil = true;
            model.NoPupilSelected = true;
            return await ReturnToSearch(model);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupil) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            PopulateConfirmationNavigation(model.StarredPupilConfirmationViewModel);
            model.StarredPupilConfirmationViewModel.DownloadType = DownloadType.NPD;
            model.StarredPupilConfirmationViewModel.SelectedPupil = selectedPupil;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        return await DownloadSelectedNationalPupilDatabaseData(selectedPupil, model.SearchText);
    }

    [Route(Routes.NationalPupilDatabase.LearnerTextDownloadOptions)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
        string selectedPupil,
        string searchText)
    {
        var searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupil,
            LearnerNumber = selectedPupil,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        SearchDownloadHelper.AddDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationLowAge(), User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(), User.IsOrganisationAllAges());

        ModelState.Clear();

        searchDownloadViewModel.LearnerNumber = selectedPupil;
        searchDownloadViewModel.SearchAction = Global.NPDAction;
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerTextDownloadFile;
        searchDownloadViewModel.RedirectRoute = Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;
        searchDownloadViewModel.TextSearchViewModel = new LearnerTextSearchViewModel() { LearnerNumberLabel = LearnerNumberLabel, SearchText = searchText };
        PopulateNavigation(searchDownloadViewModel.TextSearchViewModel);

        var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
        selectedPupil = PupilHelper.CheckIfStarredPupil(selectedPupil) ? RbacHelper.DecryptUpn(selectedPupil) : selectedPupil;
        var sortOrder = new string[] { ValidationHelper.IsValidUpn(selectedPupil) ? selectedPupil : "0" };


        var disabledTypes = await _downloadService.CheckForNoDataAvailable(new string[] { selectedPupil },
            sortOrder,
            downloadTypeArray, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
        SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.NonLearnerNumberDownloadOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.NationalPupilDatabase.LearnerTextDownloadFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!String.IsNullOrEmpty(model.SelectedPupils))
        {
            var selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupils) ? RbacHelper.DecryptUpn(model.SelectedPupils) : model.SelectedPupils;
            var sortOrder = new string[] { ValidationHelper.IsValidUpn(selectedPupil) ? selectedPupil : "0" };

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = SearchErrorMessages.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = model.DownloadFileType == DownloadFileType.CSV ?
                    await _downloadService.GetCSVFile(new string[] { selectedPupil }, sortOrder, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase).ConfigureAwait(false) :
                    await _downloadService.GetTABFile(new string[] { selectedPupil }, sortOrder, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase).ConfigureAwait(false);

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
            if (this.HttpContext.Session.Keys.Contains(SearchSessionKey))
                model.TextSearchViewModel.SearchText = this.HttpContext.Session.GetString(SearchSessionKey);

            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.TextSearchViewModel.SearchText);
        }

        return RedirectToAction(SearchAction, SearchController);
    }


    [HttpPost]
    [Route(Routes.NPDNonUpnInvalidUPNs)]
    public async Task<IActionResult> NonUpnInvalidUPNs(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNs(model);
    }

    [HttpPost]
    [Route(Routes.NPDNonUpnInvalidUPNsConfirmation)]
    public async Task<IActionResult> NonUpnInvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNsConfirmation(model);
    }
}
