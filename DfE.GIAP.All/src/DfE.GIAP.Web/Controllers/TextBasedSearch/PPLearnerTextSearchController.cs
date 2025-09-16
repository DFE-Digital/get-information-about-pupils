using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch;

[Route(Routes.Application.Search)]
public class PPLearnerTextSearchController : BaseLearnerTextSearchController
{
    private readonly ILogger<PPLearnerTextSearchController> _logger;
    private readonly IDownloadService _downloadService;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;

    public override string PageHeading => ApplicationLabels.SearchPupilPremiumWithOutUpnPageHeading;
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

    public override string DownloadSelectedLink => ApplicationLabels.DownloadSelectedPupilPremiumDataLink;


    public PPLearnerTextSearchController(
        ILogger<PPLearnerTextSearchController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IPaginatedSearchService paginatedSearch,
        ITextSearchSelectionManager selectionManager,
        ISessionProvider sessionProvider,
        IDownloadService downloadService,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase)
        : base(logger, paginatedSearch, selectionManager, azureAppSettings, sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;
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
        PopulatePageText(model);
        PopulateNavigation(model);
        SetSortOptions(model);

        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        string selectedUpn = GetSelected();

        if (string.IsNullOrEmpty(selectedUpn))
        {
            model.NoPupil = true;
            model.NoPupilSelected = true;
            model.ErrorDetails = Messages.Common.Errors.NoPupilsSelected;
            return await ReturnToSearch(model);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedUpn))
        {
            selectedUpn = RbacHelper.DecodeUpn(selectedUpn);
        }

        if (!ValidationHelper.IsValidUpn(selectedUpn)) // TODO can we surface invalid UPNs?
        {
            return await InvalidUPNs(new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = selectedUpn
            });
        }

        try
        {
            string userId = User.GetUserId();

            AddPupilsToMyPupilsRequest addRequest = new(
                userId: userId,
                pupils: [selectedUpn]);

            await _addPupilsToMyPupilsUseCase.HandleRequestAsync(addRequest);
        }

        catch (MyPupilsLimitExceededException) // TODO domain exception bleeding through. Result Pattern? Decision: Preserve existing behaviour
        {
            model.ErrorDetails = Messages.Common.Errors.MyPupilListLimitExceeded;
            return await ReturnToSearch(model);
        }

        model.ItemAddedToMyPupilList = true;
        return await ReturnToSearch(model);
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

        var selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupils) ? RbacHelper.DecodeUpn(model.SelectedPupils) : model.SelectedPupils;
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
            model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
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
            model.ErrorDetails = Messages.Downloads.Errors.NoPupilSelected;
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
