using System.Globalization;
using System.Text.Json;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Helpers.Controllers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;

[Route(Routes.Application.Search)]
public sealed class PupilPremiumLearnerTextSearchController : Controller
{
    private const int PAGESIZE = 20;
    private const string PersistedSelectedSexFiltersKey = "PersistedSelectedSexFilters";
    private readonly ILogger<PupilPremiumLearnerTextSearchController> _logger;
    private readonly IPaginatedSearchService _paginatedSearchService;
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IUseCase<PupilPremiumSearchRequest, PupilPremiumSearchResponse> _useCase;
    private readonly ITextSearchSelectionManager _selectionManager;
    private readonly ISessionProvider _sessionProvider;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupils;

    public string PageHeading => ApplicationLabels.SearchPupilPremiumWithOutUpnPageHeading;
    public string LearnerNumberLabel => Global.LearnerNumberLabel;

    public string SearchSessionKey => Global.PPNonUpnSearchSessionKey;
    public string SearchFiltersSessionKey => Global.PPNonUpnSearchFiltersSessionKey;

    public string SortDirectionKey => Global.PPNonUpnSortDirectionSessionKey;
    public string SortFieldKey => Global.PPNonUpnSortFieldSessionKey;

    public string SearchView => Global.NonUpnSearchView;
    public string DownloadLinksPartial => Global.PPNonUpnDownloadLinksView;
    public string SearchLearnerNumberAction => Routes.PupilPremium.PupilPremiumDatabase;
    public string RedirectUrlFormAction => Global.PPNonUpnAction;
    public string LearnerTextDatabaseName => Global.PPLearnerTextSearchDatabaseName;
    public string RedirectFrom => Routes.PupilPremium.NonUPN;

    public string SurnameFilterUrl => Routes.PupilPremium.NonUPNSurnameFilter;
    public string DobFilterUrl => Routes.PupilPremium.NonUpnDobFilter;
    public string ForenameFilterUrl => Routes.PupilPremium.NonUpnForenameFilter;
    public string MiddlenameFilterUrl => Routes.PupilPremium.NonUpnMiddlenameFilter;

    public string SexFilterUrl => Routes.PupilPremium.NonUpnSexFilter;
    public string FormAction => Routes.PupilPremium.NonUPN;
    public string RemoveActionUrl => $"/{Routes.Application.Search}/{Routes.PupilPremium.NonUPN}";
    public AzureSearchIndexType IndexType => AzureSearchIndexType.PupilPremium;
    public string SearchAction => Global.PPNonUpnAction;
    public string SearchController => Global.PPTextSearchController;
    public ReturnRoute ReturnRoute => ReturnRoute.NonPupilPremium;

    public string DownloadSelectedLink => ApplicationLabels.DownloadSelectedPupilPremiumDataLink;

    public PupilPremiumLearnerTextSearchController(
        ILogger<PupilPremiumLearnerTextSearchController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IPaginatedSearchService paginatedSearchService,
        ITextSearchSelectionManager selectionManager,
        ISessionProvider sessionProvider,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupils)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(paginatedSearchService);
        _paginatedSearchService = paginatedSearchService;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _azureAppSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupils);
        _downloadPupilPremiumDataForPupils = downloadPupilPremiumDataForPupils;
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

        SetSelections(model.SelectedPupil);

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

        if (!ValidationHelper.IsValidUpn(selectedUpn))
        {
            model.ErrorDetails = Messages.Common.Errors.InvalidPupilIdentifier;
            return await ReturnToSearch(model);
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
    public async Task<IActionResult> DownloadPupilPremiumFile(LearnerDownloadViewModel model, CancellationToken ctx = default)
    {
        string selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupils) ? RbacHelper.DecodeUpn(model.SelectedPupils) : model.SelectedPupils;

        DownloadPupilPremiumFilesResponse result = await _downloadPupilPremiumDataForPupils.DownloadAsync(
            pupilUpns: [selectedPupil],
            downloadEventType: Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
            ctx);

        if (result.HasData)
        {
            return result.GetResult();
        }

        model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
        return RedirectToAction(Global.PPNonUpnAction, Global.PPTextSearchController);
    }

    [Route(Routes.PupilPremium.LearnerTextDownloadRequest)]
    public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(LearnerTextSearchViewModel model)
    {
        SetSelections(model.SelectedPupil);

        string selectedPupil = GetSelected();

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

        LearnerDownloadViewModel searchDownloadViewModel = new LearnerDownloadViewModel
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
            LearnerDownloadViewModel searchDownloadViewModel = new LearnerDownloadViewModel
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

    #region WIP legacy inherited methods to deprecate

    private async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerTextSearchViewModel model = new();

        PopulatePageText(model);
        PopulateNavigation(model);
        model.LearnerNumberLabel = LearnerNumberLabel;

        if (returnToSearch ?? false)
        {
            if (_sessionProvider.ContainsSessionKey(SearchSessionKey))
            {
                model.SearchText = _sessionProvider.GetSessionValue(SearchSessionKey);
            }
            if (_sessionProvider.ContainsSessionKey(SearchFiltersSessionKey))
            {
                model.SearchFilters = _sessionProvider.GetSessionValueOrDefault<SearchFilters>(SearchFiltersSessionKey);
            }

            SetSortOptions(model);
            GetPersistedSexFiltersForViewModel(model);
            model = await GenerateLearnerTextSearchViewModel(model, null, null, null, null,
                model.SortField,
                model.SortDirection);
            model.PageNumber = 0;
            model.PageSize = PAGESIZE;
        }

        if (!returnToSearch.HasValue)
        {
            _selectionManager.Clear();
        }

        return View(Global.NonUpnSearchView, model);
    }

    private async Task<IActionResult> Search(
        LearnerTextSearchViewModel model,
        string surnameFilter, string middlenameFilter,
        string forenameFilter, string searchByRemove,
        int pageNumber,
        bool hasQueryItem = false,
        bool calledByController = false,
        string sortField = "",
        string sortDirection = "",
        bool resetSelection = false)
    {
        GetPersistedSexFiltersForViewModel(model);
        model.SearchText = SecurityHelper.SanitizeText(model.SearchText);
        model.LearnerNumberLabel = LearnerNumberLabel;
        bool notPaged = hasQueryItem || calledByController;

        if (notPaged && !model.NoPupilSelected)
        {
            SetSelections(model.SelectedPupil);
        }

        if (resetSelection || searchByRemove != null)
        {
            _selectionManager.Clear();
            ClearSortOptions();
            RemoveSexFilterItemFromModel(model, searchByRemove);
            SetPersistedSexFiltersForViewModel(model);
        }

        if (resetSelection && searchByRemove == null)
        {
            RemoveAllSexFilterItemsFromModel(model);
        }

        model.PageNumber = pageNumber;
        model.PageSize = PAGESIZE;

        if (!string.IsNullOrEmpty(sortField) || !string.IsNullOrEmpty(sortDirection))
        {
            HttpContext.Session.SetString(SortFieldKey, sortField);
            HttpContext.Session.SetString(SortDirectionKey, sortDirection);
        }

        SetSortOptions(model);
        if (!string.IsNullOrEmpty(model.SearchText))
        {
            model = await GenerateLearnerTextSearchViewModel(
            model,
            surnameFilter,
            middlenameFilter,
            forenameFilter,
            searchByRemove,
            model.SortField,
            model.SortDirection);
        }

        model.ReturnRoute = ReturnRoute;

        PopulatePageText(model);
        PopulateNavigation(model);

        _sessionProvider.SetSessionValue(SearchSessionKey, model.SearchText);

        if (model.SearchFilters != null)
        {
            _sessionProvider.SetSessionValue(SearchFiltersSessionKey, model.SearchFilters);
        }

        return View(SearchView, model);
    }

    private async Task<IActionResult> ReturnToSearch(LearnerTextSearchViewModel model)
    {
        if (_sessionProvider.ContainsSessionKey(SearchSessionKey))
        {
            model.SearchText = _sessionProvider.GetSessionValue(SearchSessionKey);
        }
        if (_sessionProvider.ContainsSessionKey(SearchFiltersSessionKey))
        {
            model.SearchFilters = _sessionProvider.GetSessionValueOrDefault<SearchFilters>(SearchFiltersSessionKey);
        }

        return await Search(model, null, null, null, null, model.PageNumber, calledByController: true, hasQueryItem: true, sortField: model.SortField, sortDirection: model.SortDirection);
    }

    private async Task<IActionResult> DobSearchFilter(LearnerTextSearchViewModel model)
    {
        int day = model.SearchFilters.CustomFilterText.DobDay;
        int month = model.SearchFilters.CustomFilterText.DobMonth;
        int year = model.SearchFilters.CustomFilterText.DobYear;

        ModelState.Clear();

        if (day == 0 && month == 0 && year == 0)
        {
            ModelState.AddModelError("DobEmpty", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.DobErrorEmpty = true;
            model.FilterErrors.DobError = true;
        }
        else if (day != 0 && month == 0 && year == 0)
        {
            ModelState.AddModelError("DayOnly", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.DobErrorDayOnly = true;
            model.FilterErrors.DobError = true;
        }
        else if (day != 0 && month != 0 && year == 0)
        {
            ModelState.AddModelError("DayMonthOnly", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.DobErrorDayMonthOnly = true;
            model.FilterErrors.DobError = true;
        }
        else if (day < 0 || day > 31)
        {
            ModelState.AddModelError("DayOutOfRange", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.DayOutOfRange = true;
            model.FilterErrors.DobError = true;
        }
        else if (day == 0 && month != 0 && year == 0)
        {
            ModelState.AddModelError("MonthOnly", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.DobErrorMonthOnly = true;
            model.FilterErrors.DobError = true;
        }
        else if (day != 0 && month == 0 && year != 0)
        {
            ModelState.AddModelError("NoMonth", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.DobErrorNoMonth = true;
            model.FilterErrors.DobError = true;
        }

        if (!model.FilterErrors.DobError && (month < 0 || month > 12))
        {
            ModelState.AddModelError("MonthOutOfRange", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.MonthOutOfRange = true;
            model.FilterErrors.DobError = true;
        }

        if (!model.FilterErrors.DobError && (year < 0 || year > 0))
        {
            int yearLimit = DateTime.Now.Year - 3;
            if (year > yearLimit)
            {
                ModelState.AddModelError("YearLimitHigh", Messages.Search.Errors.DobInvalid);
                model.FilterErrors.YearLimitHigh = true;
                model.FilterErrors.DobError = true;
            }
            else if (year < Global.YearMinLimit)
            {
                ModelState.AddModelError("YearLimitLow", Messages.Search.Errors.DobInvalid);
                model.FilterErrors.YearLimitLow = true;
                model.FilterErrors.DobError = true;
            }
        }

        if (!model.FilterErrors.DobError && (day > 0 && month > 0 && year > 0) && !PupilHelper.IsValidateDate($"{day.ToString("00")}/{month.ToString("00")}/{year}"))
        {
            ModelState.AddModelError("InvalidDate", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.InvalidDob = true;
            model.FilterErrors.DobError = true;
        }

        return await ReturnToRoute(model).ConfigureAwait(false);
    }


    private async Task<IActionResult> SurnameSearchFilter(LearnerTextSearchViewModel model, string surnameFilter)
    {
        ModelState.Clear();

        if (!string.IsNullOrEmpty(surnameFilter))
        {
            model.SearchFilters.CustomFilterText.Surname = SecurityHelper.SanitizeText(surnameFilter);
        }

        if (string.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Surname))
        {
            ModelState.AddModelError("NoSurnameFilter", Messages.Search.Errors.FilterEmpty);
            model.FilterErrors.SurnameError = true;
        }

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    private async Task<IActionResult> MiddlenameSearchFilter(LearnerTextSearchViewModel model, string middlenameFilter)
    {
        ModelState.Clear();

        if (!string.IsNullOrEmpty(middlenameFilter))
        {
            model.SearchFilters.CustomFilterText.Middlename = SecurityHelper.SanitizeText(middlenameFilter);
        }

        if (string.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Middlename))
        {
            ModelState.AddModelError("NoMiddlenameFilter", Messages.Search.Errors.FilterEmpty);
            model.FilterErrors.MiddlenameError = true;
        }

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    private async Task<IActionResult> ForenameSearchFilter(LearnerTextSearchViewModel model, string forenameFilter)
    {
        ModelState.Clear();

        if (!string.IsNullOrEmpty(forenameFilter))
        {
            model.SearchFilters.CustomFilterText.Forename = SecurityHelper.SanitizeText(forenameFilter);
        }

        if (string.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Forename))
        {
            ModelState.AddModelError("NoForenameFilter", Messages.Search.Errors.FilterEmpty);
            model.FilterErrors.ForenameError = true;
        }

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [NonAction]
    public async Task<IActionResult> SexSearchFilter(LearnerTextSearchViewModel model)
    {
        SetPersistedSexFiltersForViewModel(model);
        ModelState.Clear();
        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    protected async Task<IActionResult> ReturnToRoute(LearnerTextSearchViewModel model)
    {
        _selectionManager.Clear();
        ClearSortOptions();

        return await Search(model, null, null, null, null, model.PageNumber, false);
    }

    protected void GetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model)
    {
        string[] sexFilters =
            TempData.GetPersistedObject<string[]>(
                PersistedSelectedSexFiltersKey,
                keepTempDataBetweenRequests: true);

        if (sexFilters != null)
            model.SelectedSexValues = sexFilters;
    }

    protected void SetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model) =>
        TempData.SetPersistedObject(
            model.SelectedSexValues,
            PersistedSelectedSexFiltersKey);

    protected void RemoveSexFilterItemFromModel(
        LearnerTextSearchViewModel model,
        string sexFilterItem)
    {
        if (!string.IsNullOrWhiteSpace(sexFilterItem))
        {
            model.SelectedSexValues =
                model.SelectedSexValues?.Where(selectedSexValue =>
                    selectedSexValue.SwitchSexToParseName() != sexFilterItem).ToArray();
        }
    }

    protected void RemoveAllSexFilterItemsFromModel(
        LearnerTextSearchViewModel model)
    {
        model.SelectedSexValues = null;
        SetPersistedSexFiltersForViewModel(model);
        TempData.Remove(PersistedSelectedSexFiltersKey);
    }

    private void SetSortOptions(LearnerTextSearchViewModel model)
    {
        if (HttpContext.Session.Keys.Contains(SortDirectionKey))
            model.SortDirection = HttpContext.Session.GetString(SortDirectionKey);
        if (HttpContext.Session.Keys.Contains(SortFieldKey))
            model.SortField = HttpContext.Session.GetString(SortFieldKey);
    }

    private void ClearSortOptions()
    {
        HttpContext.Session.Remove(SortDirectionKey);
        HttpContext.Session.Remove(SortFieldKey);
    }


    private LearnerTextSearchViewModel PopulatePageText(LearnerTextSearchViewModel model)
    {
        model.PageHeading = PageHeading;
        model.ShowMiddleNames = true;
        model.ShowLocalAuthority = true;
        return model;
    }

    private LearnerTextSearchViewModel PopulateNavigation(LearnerTextSearchViewModel model)
    {
        model.LearnerTextSearchController = SearchController;
        model.LearnerTextSearchAction = SearchAction;

        model.LearnerNumberController = Routes.Application.Search;
        model.LearnerNumberAction = SearchLearnerNumberAction;
        model.RedirectUrls.FormAction = RedirectUrlFormAction;

        model.LearnerTextDatabaseName = LearnerTextDatabaseName;
        model.RedirectUrls.DobFilterUrl = DobFilterUrl;
        model.RedirectFrom = RedirectFrom;
        model.DownloadLinksPartial = DownloadLinksPartial;

        return model;
    }

    private void SetSelections(string selected)
    {
        if (!string.IsNullOrEmpty(selected))
        {
            _selectionManager.Clear();
            _selectionManager.Add(selected);
        }
    }

    private string GetSelected()
    {
        return _selectionManager.GetSelectedFromSession();
    }


    [NonAction]
    public IActionResult ConfirmationForStarredPupil(StarredPupilConfirmationViewModel model)
    {
        LearnerTextSearchViewModel searchViewModel = new LearnerTextSearchViewModel()
        {
            SearchText = HttpContext.Session.Keys.Contains(SearchSessionKey) ? HttpContext.Session.GetString(SearchSessionKey) : string.Empty,
            LearnerTextSearchController = SearchController,
            LearnerTextSearchAction = SearchAction,
            ShowStarredPupilConfirmation = true,
            StarredPupilConfirmationViewModel = model,
            LearnerNumberLabel = LearnerNumberLabel
        };
        PopulateNavigation(searchViewModel);
        SetSortOptions(searchViewModel);
        PopulatePageText(searchViewModel);
        return View(SearchView, searchViewModel);
    }

    protected async Task<LearnerTextSearchViewModel> GenerateLearnerTextSearchViewModel(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string foremameFilter,
        string searchByRemove,
        string sortField = "",
        string sortDirection = "")
    {
        List<CurrentFilterDetail> currentFilters = SetCurrentFilters(model, surnameFilter, middlenameFilter, foremameFilter, searchByRemove);

        model.LearnerTextDatabaseName = LearnerTextDatabaseName;

        model = SetSearchFiltersUrls(model);

        if (ModelState.IsValid)
        {
            model.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
            model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
            model.DownloadSelectedLink = DownloadSelectedLink;


            if (currentFilters.Count > 0)
            {
                model.SearchFilters.CurrentFiltersApplied = currentFilters;
            }

            if (currentFilters != null)
            {
                model.SearchFilters.CurrentFiltersAppliedString = JsonSerializer.Serialize(currentFilters);
            }
        }

        bool hasCustomFilters = (model.SearchFilters.CustomFilterText.Surname != null ||
            model.SearchFilters.CustomFilterText.Forename != null ||
            model.SearchFilters.CustomFilterText.Middlename != null ||
            model.SearchFilters.CustomFilterText.DobDay != 0 ||
            model.SearchFilters.CustomFilterText.DobMonth != 0 ||
            model.SearchFilters.CustomFilterText.DobYear != 0) ? true : false;

        bool first = hasCustomFilters || model.PageNumber != 0 ? false : true;
        model = await GetPupilsByNameForSearchBuilder(
            model,
            IndexType,
            currentFilters,
            model.PageNumber,
            first,
            sortField,
            sortDirection).ConfigureAwait(false);

        return model;
    }

    private async Task<LearnerTextSearchViewModel> GetPupilsByNameForSearchBuilder(
        LearnerTextSearchViewModel model,
        AzureSearchIndexType indexType,
        List<CurrentFilterDetail> currentFilters,
        int pageNumber,
        bool first,
        string sortField = "",
        string sortDirection = "")
    {
        Dictionary<string, string[]> requestFilters = GenerateRequestModel(model, currentFilters);

        PaginatedResponse result = await _paginatedSearchService.GetPage(
            model.SearchText,
            requestFilters,
            first ? _azureAppSettings.MaximumNonUPNResults : PAGESIZE,
            pageNumber,
            indexType,
            AzureSearchQueryType.Text,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            sortField,
            sortDirection
          );

        ParseSex(ref result);

        int lowAge = User.GetOrganisationLowAge();
        int highAge = User.GetOrganisationHighAge();

        model.Learners = result.Learners;
        model.Count = (int)result.Count;
        model.Total = result.Count ?? result.Learners.Count;

        model.Filters = result.Filters;

        SetLearnerNumberId(model);

        bool isAdmin = User.IsAdmin();
        if (!isAdmin && indexType != AzureSearchIndexType.FurtherEducation)
        {
            model.Learners = RbacHelper.CheckRbacRulesGeneric<Learner>(model.Learners.ToList(), lowAge, highAge);
        }

        string selected = GetSelected();

        if (!string.IsNullOrEmpty(selected))
        {
            foreach (Learner learner in model.Learners)
            {
                if (!string.IsNullOrEmpty(learner.LearnerNumberId))
                {
                    learner.Selected = selected.Contains(learner.LearnerNumberId);
                }
            }
        }

        model.Learners = first ? result.Learners.Take(PAGESIZE) : result.Learners;

        model.PageLearnerNumbers = string.Join(',', model.Learners.Select(l => l.LearnerNumberId));

        model.ShowOverLimitMessage = model.Total > 100000;

        return model;
    }

    private static Dictionary<string, string[]> GenerateRequestModel(LearnerTextSearchViewModel model, List<CurrentFilterDetail> currentFilters)
    {
        Dictionary<string, string[]> requestFilters = [];
        List<string> surnameList = [];
        List<string> middlenameList = [];
        List<string> forenameList = [];
        List<string> dobList = [];

        if (currentFilters != null)
        {
            foreach (CurrentFilterDetail filter in currentFilters)
            {
                if (filter.FilterType == FilterType.Surname)
                {
                    surnameList.Add(filter.FilterName);
                }
                if (filter.FilterType == FilterType.MiddleName)
                {
                    middlenameList.Add(filter.FilterName);
                }
                if (filter.FilterType == FilterType.Forename)
                {
                    forenameList.Add(filter.FilterName);
                }
                if (filter.FilterType == FilterType.Dob)
                {
                    if (!model.FilterErrors.DobError)
                    {
                        if (model.SearchFilters.CustomFilterText.DobDay == 0 && model.SearchFilters.CustomFilterText.DobMonth == 0 &&
                            model.SearchFilters.CustomFilterText.DobYear == 0)
                        {
                            PupilHelper.ConvertFilterNameToCustomDOBFilterText(filter.FilterName, out int dobDay, out int dobMonth, out int dobYear);
                            model.SearchFilters.CustomFilterText.DobDay = dobDay;
                            model.SearchFilters.CustomFilterText.DobMonth = dobMonth;
                            model.SearchFilters.CustomFilterText.DobYear = dobYear;
                        }

                        if (model.SearchFilters.CustomFilterText.DobDay > 0 && model.SearchFilters.CustomFilterText.DobMonth > 0 && model.SearchFilters.CustomFilterText.DobYear > 0)
                        {
                            dobList.Add(DateTime.Parse(filter.FilterName, new CultureInfo("en-GB")).ToString("yyy-MM-dd", new CultureInfo("en-GB")));
                            requestFilters.Add("dob", dobList.ToArray());
                        }
                        else
                        {
                            if (model.SearchFilters.CustomFilterText.DobMonth == 0)
                                requestFilters.Add("dobyear", new string[] { model.SearchFilters.CustomFilterText.DobYear.ToString() });
                            else
                            {
                                requestFilters.Add("dobmonth", new string[] { model.SearchFilters.CustomFilterText.DobMonth.ToString() });
                                requestFilters.Add("dobyear", new string[] { model.SearchFilters.CustomFilterText.DobYear.ToString() });
                            }
                        }
                    }
                }

                if (filter.FilterType == FilterType.Sex)
                {
                    List<string> currentSelectedSexList = [];
                    if (model.SelectedSexValues != null)
                    {
                        currentSelectedSexList = model.SelectedSexValues.ToList();
                    }

                    if (!currentSelectedSexList.Any(x => x.Equals(filter.FilterName.Substring(0, 1))))
                    {
                        currentSelectedSexList.Add(filter.FilterName.Substring(0, 1));
                    }
                    model.SelectedSexValues = [.. currentSelectedSexList];
                }
            }
        }

        if (surnameList.ToArray().Length > 0)
            requestFilters.Add("surname", surnameList.ToArray());

        if (middlenameList.ToArray().Length > 0)
            requestFilters.Add("middlenames", middlenameList.ToArray());

        if (forenameList.ToArray().Length > 0)
            requestFilters.Add("forename", forenameList.ToArray());

        if (model.SelectedSexValues != null && model.SelectedSexValues.Length > 0)
            requestFilters.Add("sex", model.SelectedSexValues.ToArray());

        return requestFilters;
    }

    private static void ParseSex(ref PaginatedResponse result)
    {
        List<FilterData> sexFilter = result.Filters.Where(
            (filterData) =>
                filterData.Name.Equals("Sex")).ToList();

        sexFilter.ForEach(
            (filterData) =>
                filterData.Items.ForEach(
                    (filterDataItem) =>
                        filterDataItem.Value = filterDataItem.Value.SwitchSexToParseName()));
    }

    private static List<CurrentFilterDetail> CheckTextFilters(LearnerTextSearchViewModel model, List<CurrentFilterDetail> currentFilters,
      string surnameFilter, string middlenameFilter, string forenameFilter)
    {
        if (forenameFilter != null)
        {
            AddNameFilter(ref currentFilters, FilterType.Forename, forenameFilter);
        }

        if (middlenameFilter != null)
        {
            AddNameFilter(ref currentFilters, FilterType.MiddleName, middlenameFilter);
        }

        if (surnameFilter != null)
        {
            AddNameFilter(ref currentFilters, FilterType.Surname, surnameFilter);
        }

        if (model.SearchFilters.CustomFilterText.Forename != null)
        {
            AddNameFilter(ref currentFilters, FilterType.Forename, model.SearchFilters.CustomFilterText.Forename);
        }

        if (model.SearchFilters.CustomFilterText.Middlename != null)
        {
            AddNameFilter(ref currentFilters, FilterType.MiddleName, model.SearchFilters.CustomFilterText.Middlename);
        }

        if (model.SearchFilters.CustomFilterText.Surname != null)
        {
            AddNameFilter(ref currentFilters, FilterType.Surname, model.SearchFilters.CustomFilterText.Surname);
        }
        return currentFilters;
    }

    private static void AddNameFilter(ref List<CurrentFilterDetail> currentFilters, FilterType filterType, string filterValue)
    {
        if (!currentFilters.Any(x => x.FilterType == filterType && x.FilterName.Equals(filterValue)))
        {
            currentFilters.Add
            (
                new CurrentFilterDetail
                {
                    FilterType = filterType,
                    FilterName = filterValue.ToLowerInvariant()
                }
            );
        }
    }

    private static List<CurrentFilterDetail> RemoveFilterValue(string searchByRemove, List<CurrentFilterDetail> currentFilters, LearnerTextSearchViewModel model)
    {
        if (!string.IsNullOrEmpty(searchByRemove))
        {
            CurrentFilterDetail item = currentFilters.Find((x) => x.FilterName == searchByRemove);

            if (item != null)
            {
                currentFilters.Remove(item);
            }

            CurrentFilterDetail sexFiltersActive = currentFilters.Find(x => x.FilterType == FilterType.Sex);

            if (sexFiltersActive != null &&
                    model.SelectedSexValues == null &&
                        currentFilters.Count >= 1)
            {
                List<string> currentSelectedSexList = [];
                foreach (CurrentFilterDetail filter in currentFilters)
                {
                    currentSelectedSexList.Add(filter.FilterName.Substring(0, 1));
                }
                model.SelectedSexValues = [.. currentSelectedSexList];
            }
        }
        return currentFilters;
    }


    private List<CurrentFilterDetail> SetCurrentFilters(LearnerTextSearchViewModel model,
   string surnameFilter, string middlenameFilter, string forenameFilter, string searchByRemove)
    {
        List<CurrentFilterDetail> currentFilters = !string.IsNullOrEmpty(model.SearchFilters.CurrentFiltersAppliedString)
                                                   ? JsonSerializer.Deserialize<List<CurrentFilterDetail>>(model.SearchFilters.CurrentFiltersAppliedString)
                                                   : [];

        currentFilters = CheckDobFilter(model, currentFilters);
        currentFilters = CheckSexFilter(model, currentFilters);

        if (!string.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Forename) ||
            !string.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Middlename) ||
            !string.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Surname) ||
            !string.IsNullOrEmpty(searchByRemove) ||
            !string.IsNullOrEmpty(surnameFilter) ||
            !string.IsNullOrEmpty(forenameFilter) ||
            !string.IsNullOrEmpty(middlenameFilter))
        {
            currentFilters = RemoveFilterValue(searchByRemove, currentFilters, model);
            currentFilters = CheckTextFilters(model, currentFilters, surnameFilter, middlenameFilter, forenameFilter);
        }
        return currentFilters.ToList();
    }

    private LearnerTextSearchViewModel SetSearchFiltersUrls(LearnerTextSearchViewModel model)
    {
        model.RedirectUrls.SurnameFilterURL = SurnameFilterUrl;
        model.RedirectUrls.FormAction = FormAction;
        model.RedirectUrls.RemoveAction = RemoveActionUrl;
        model.RedirectUrls.DobFilterUrl = DobFilterUrl;
        model.RedirectUrls.ForenameFilterUrl = ForenameFilterUrl;
        model.RedirectUrls.MiddlenameFilterUrl = MiddlenameFilterUrl;
        model.RedirectUrls.SexFilterUrl = SexFilterUrl;

        return model;
    }

    private static List<CurrentFilterDetail> CheckSexFilter(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters)
    {
        if (model.SelectedSexValues?.Length > 0)
        {
            RemoveAllSexFilters(currentFilters);
            AddSelectedSexValuesToCurrentFilters(model, currentFilters);
        }

        else if (model.SelectedSexValues == null && currentFilters.Count > 0)
        {
            RemoveSelectedFilterValueFromCurrentFilters(currentFilters, model);
            model.SelectedSexValues = null;
        }

        return currentFilters;
    }

    private static void AddSelectedSexValuesToCurrentFilters(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters)
    {
        model.SelectedSexValues.Distinct().ToList().ForEach(sexFilterItem =>
        {
            currentFilters.Add(
                new CurrentFilterDetail
                {
                    FilterType = FilterType.Sex,
                    FilterName = sexFilterItem.SwitchSexToParseName()
                });
        });
    }

    private static void RemoveAllSexFilters(
        List<CurrentFilterDetail> currentFilters) =>
        currentFilters.RemoveAll(currentFilterDetail =>
            currentFilterDetail.FilterType == FilterType.Sex);

    private static IEnumerable<string> ExtractSexValuesFromCurrentFilterDetail(
        IEnumerable<CurrentFilterDetail> currentFilters) =>
        currentFilters.Where(currentFilterDetail =>
                currentFilterDetail.FilterType == FilterType.Sex)
            .Select(currentFilterDetail =>
                currentFilterDetail.FilterName);

    private static void RemoveSelectedFilterValueFromCurrentFilters(
        List<CurrentFilterDetail> currentFilters,
        LearnerTextSearchViewModel model)
    {
        IEnumerable<string> currentFiltersSex =
            ExtractSexValuesFromCurrentFilterDetail(currentFilters);

        currentFiltersSex.ToList().ForEach(
            (sex) => currentFilters = RemoveFilterValue(sex, currentFilters, model));
    }

    private static List<CurrentFilterDetail> CheckDobFilter(LearnerTextSearchViewModel model, List<CurrentFilterDetail> currentFilters)
    {
        if (model.SearchFilters.CustomFilterText.DobDay > 0 && model.SearchFilters.CustomFilterText.DobMonth > 0 &&
               model.SearchFilters.CustomFilterText.DobYear > 0)
        {
            currentFilters.RemoveAll(x => x.FilterType == FilterType.Dob);

            currentFilters.Add(new CurrentFilterDetail
            {
                FilterType = FilterType.Dob,
                FilterName = $"{model.SearchFilters.CustomFilterText.DobDay}/{model.SearchFilters.CustomFilterText.DobMonth}/" +
                                $"{model.SearchFilters.CustomFilterText.DobYear}"
            });
        }
        else if (model.SearchFilters.CustomFilterText.DobDay == 0 && model.SearchFilters.CustomFilterText.DobMonth > 0 &&
               model.SearchFilters.CustomFilterText.DobYear > 0)
        {
            currentFilters.RemoveAll(x => x.FilterType == FilterType.Dob);

            currentFilters.Add(new CurrentFilterDetail
            {
                FilterType = FilterType.Dob,
                FilterName = $"{model.SearchFilters.CustomFilterText.DobMonth}/" +
                                                $"{model.SearchFilters.CustomFilterText.DobYear}"
            });
        }
        else if (model.SearchFilters.CustomFilterText.DobDay == 0 && model.SearchFilters.CustomFilterText.DobMonth == 0 &&
                    model.SearchFilters.CustomFilterText.DobYear > 0)
        {
            currentFilters.RemoveAll(x => x.FilterType == FilterType.Dob);

            currentFilters.Add(new CurrentFilterDetail
            {
                FilterType = FilterType.Dob,
                FilterName = $"{model.SearchFilters.CustomFilterText.DobYear}"
            });
        }

        return currentFilters;
    }



    private static void SetLearnerNumberId(LearnerTextSearchViewModel model)
    {

        foreach (Learner learner in model.Learners)
        {
            learner.LearnerNumberId = learner.LearnerNumber switch
            {
                "0" => learner.Id,
                _ => learner.LearnerNumber,
            };

        }

    }

    #endregion
}
