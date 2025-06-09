using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch;

[Route(Routes.Application.Search)]
public class PPLearnerTextSearchController : BaseLearnerTextSearchController
{
    private readonly ILogger<PPLearnerTextSearchController> _logger;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    public override string PageHeading => ApplicationLabel.SearchPupilPremiumWithOutUpnPageHeading;
    public override string SearchSessionKey => Global.PPNonUpnSearchSessionKey;
    public override string SearchFiltersSessionKey => Global.PPNonUpnSearchFiltersSessionKey;

    public override string SortDirectionKey => Global.PPNonUpnSortDirectionSessionKey;
    public override string SortFieldKey => Global.PPNonUpnSortFieldSessionKey;

    public override string DownloadLinksPartial => Global.PPNonUpnDownloadLinksView;
    public override string SearchLearnerNumberAction => Routes.PupilPremium.PupilPremiumDatabase;
    public override string RedirectUrlFormAction => Global.PPNonUpnAction;
    public override string LearnerTextDatabaseAction => Global.PPNonUpnAction;
    public override string LearnerTextDatabaseName => Global.PPLearnerTextSearchDatabaseName;
    public override string RedirectFrom => Routes.PupilPremium.NonUPN;

    public override string SurnameFilterUrl => Routes.PupilPremium.NonUPNSurnameFilter;
    public override string DobFilterUrl => Routes.PupilPremium.NonUpnDobFilter;
    public override string ForenameFilterUrl => Routes.PupilPremium.NonUpnForenameFilter;
    public override string MiddlenameFilterUrl => Routes.PupilPremium.NonUpnMiddlenameFilter;
    public override string GenderFilterUrl => Routes.PupilPremium.NonUpnGenderFilter;

    public override string SexFilterUrl => Routes.PupilPremium.NonUpnSexFilter;
    public override string FormAction => Routes.PupilPremium.NonUPN;
    public override string RemoveActionUrl => $"/{Routes.Application.Search}/{Routes.PupilPremium.NonUPN}";
    public override AzureSearchIndexType IndexType => AzureSearchIndexType.PupilPremium;
    public override string SearchView => Global.NonUpnSearchView;

    public override string SearchLearnerNumberController => Routes.Application.Search;
    public override string SearchAction => Global.PPNonUpnAction;
    public override string SearchController => Global.PPNonUpnController;
    public override int MyPupilListLimit => _appSettings.NonUpnPPMyPupilListLimit;
    public override ReturnRoute ReturnRoute => Common.Enums.ReturnRoute.NonPupilPremium;
    public override string LearnerTextSearchController => Global.PPNonUpnController;
    public override string LearnerTextSearchAction => SearchAction;
    public override string LearnerNumberAction => Global.PPAction;
    public override bool ShowGender => _appSettings.PpUseGender;
    public override bool ShowLocalAuthority => _appSettings.UseLAColumn;
    public override string InvalidUPNsConfirmationAction => Global.PPNonUpnInvalidUPNsConfirmation;
    public override string LearnerNumberLabel => Global.LearnerNumberLabel;
    public override bool ShowMiddleNames => true;

    public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedPupilPremiumDataLink;


    public PPLearnerTextSearchController(ILogger<PPLearnerTextSearchController> logger,
        IPaginatedSearchService paginatedSearch,
        IMyPupilListService mplService,
        ITextSearchSelectionManager selectionManager,
        IContentService contentService,
        IDownloadService downloadService,
        IOptions<AzureAppSettings> azureAppSettings)
        : base(logger, paginatedSearch, mplService, selectionManager, contentService, azureAppSettings)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
        _appSettings = azureAppSettings.Value;
    }


    [Route(Routes.PupilPremium.NonUPN)]
    [HttpGet]
    public async Task<IActionResult> NonUpnPupilPremiumDatabase(bool? returnToSearch)
    {
        _logger.LogInformation("Pupil Premium NonUpn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.PupilPremium.NonUPN)]
    [HttpPost]
    public async Task<IActionResult> NonUpnPupilPremiumDatabase(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middleNameFilter,
        string forenameFilter,
        string searchByRemove,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool returned = false,
        bool fail = false,
        bool calledByController = false)
    {
        _logger.LogInformation("Pupil Premium NonUpn POST method called");
        model.ShowHiddenUPNWarningMessage = true;
        return await Search(model, surnameFilter, middleNameFilter, forenameFilter,
                                 searchByRemove, model.PageNumber,
                                 ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
                                 calledByController,
                                 sortField, sortDirection,
                                 ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [Route(Routes.PupilPremium.NonUpnDobFilter)]
    [HttpPost]
    public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
    {
        return await DobSearchFilter(model);
    }

    [Route(Routes.PupilPremium.NonUPNSurnameFilter)]
    [HttpPost]
    public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
    {
        return await SurnameSearchFilter(model, surnameFilter);
    }

    [Route(Routes.PupilPremium.NonUpnMiddlenameFilter)]
    [HttpPost]
    public async Task<IActionResult> MiddlenameFilter(LearnerTextSearchViewModel model, string middlenameFilter)
    {
        return await MiddlenameSearchFilter(model, middlenameFilter);
    }

    [Route(Routes.PupilPremium.NonUpnForenameFilter)]
    [HttpPost]
    public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
    {
        return await ForenameSearchFilter(model, forenameFilter);
    }

    [Route(Routes.PupilPremium.NonUpnGenderFilter)]
    [HttpPost]
    public async Task<IActionResult> GenderFilter(LearnerTextSearchViewModel model)
    {
        return await GenderSearchFilter(model);
    }

    [Route(Routes.PupilPremium.NonUpnSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        return await SexSearchFilter(model);
    }

    [HttpPost]
    [Route("add-pp-nonupn-to-my-pupil-list")]
    public async Task<IActionResult> PPAddToMyPupilList(LearnerTextSearchViewModel model)
    {
        return await AddToMyPupilList(model);
    }

    [Route(Routes.PupilPremium.DownloadPupilPremiumFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadPupilPremiumFile(LearnerDownloadViewModel model)
    {
        var userOrganisation = new UserOrganisation
        {
            IsAdmin = User.IsAdmin(),
            IsEstablishment = User.IsOrganisationEstablishment(),
            IsLa = User.IsOrganisationLocalAuthority(),
            IsMAT = User.IsOrganisationMultiAcademyTrust(),
            IsSAT = User.IsOrganisationSingleAcademyTrust()
        };

        var selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupils) ? RbacHelper.DecryptUpn(model.SelectedPupils) : model.SelectedPupils;
        var sortOrder = new string[] { ValidationHelper.IsValidUpn(selectedPupil) ? selectedPupil : "0" };

        var downloadFile = await _downloadService.GetPupilPremiumCSVFile(new string[] { selectedPupil }, sortOrder, model.TextSearchViewModel.StarredPupilConfirmationViewModel.ConfirmationGiven,
                                                                        AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NonPupilPremium, userOrganisation).ConfigureAwait(false);

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
        return RedirectToAction(Global.PPNonUpnAction, Global.PPNonUpnController);
    }

    [Route(Routes.PupilPremium.LearnerTextDownloadRequest)]
    public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(LearnerTextSearchViewModel model)
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
            model.StarredPupilConfirmationViewModel.SelectedPupil = selectedPupil;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        var searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupil,
            LearnerNumber = selectedPupil,
            ErrorDetails = model.ErrorDetails,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = false
        };

        return await DownloadPupilPremiumFile(searchDownloadViewModel);
    }

    [Route(Routes.PupilPremium.DownloadNonUPNConfirmationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationError = !model.ConfirmationGiven;
        PopulateConfirmationNavigation(model);

        if (model.ConfirmationGiven)
        {
            var searchDownloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = model.SelectedPupil,
                LearnerNumber = model.SelectedPupil,
                ErrorDetails = "No Confirmation Given for Starred Pupil",
                SelectedPupilsCount = 1,
                DownloadFileType = DownloadFileType.CSV,
                ShowTABDownloadType = false
            };

            return await DownloadPupilPremiumFile(searchDownloadViewModel);
        }

        return ConfirmationForStarredPupil(model);
    }

    [Route(Routes.PupilPremium.DownloadCancellationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadCancellationReturn(StarredPupilConfirmationViewModel model)
    {
        return await Search(true);
    }

    private void PopulateConfirmationNavigation(StarredPupilConfirmationViewModel model)
    {
        model.DownloadType = DownloadType.PupilPremium;
        model.ConfirmationReturnController = SearchController;
        model.ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction;
        model.CancelReturnController = SearchController;
        model.CancelReturnAction = Global.PPDownloadCancellationReturnAction;
    }


    [HttpPost]
    [Route(Routes.PPNonUpnInvalidUPNs)]
    public async Task<IActionResult> PPNonUpnInvalidUPNs(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNs(model);
    }

    [HttpPost]
    [Route(Routes.PPNonUpnInvalidUPNsConfirmation)]
    public async Task<IActionResult> PPNonUpnInvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNsConfirmation(model);
    }
}
