using System.Text.Json;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.Search.Options;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
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

    private readonly IUseCase<
        PupilPremiumSearchRequest,
        PupilPremiumSearchResponse> _searchUseCase;

    private readonly IMapper<
        PupilPremiumLearnerTextSearchMappingContext,
        LearnerTextSearchViewModel> _learnerSearchResponseToViewModelMapper;

    private readonly IMapper<
        Dictionary<string, string[]>,
        IList<FilterRequest>> _filtersRequestMapper;

    private readonly IMapper<
        SortOrderRequest, SortOrder> _sortOrderViewModelToRequestMapper;

    private readonly IFiltersRequestFactory _filtersRequestBuilder;
    private readonly ISearchCriteriaProvider _searchCriteriaProvider;

    public string LearnerNumberLabel => Global.LearnerNumberLabel;

    public string SearchSessionKey => Global.PPNonUpnSearchSessionKey;
    public string SearchFiltersSessionKey => Global.PPNonUpnSearchFiltersSessionKey;

    public string SortDirectionKey => Global.PPNonUpnSortDirectionSessionKey;
    public string SortFieldKey => Global.PPNonUpnSortFieldSessionKey;

    public string SearchView => Global.NonUpnSearchView;
    public string DownloadLinksPartial => Global.PPNonUpnDownloadLinksView;
    public string SearchLearnerNumberAction => Routes.PupilPremium.PupilPremiumDatabase;
    public string LearnerTextDatabaseName => Global.PPLearnerTextSearchDatabaseName;

    public string DobFilterUrl => Routes.PupilPremium.NonUpnDobFilter;

    public string SearchAction => Global.PPNonUpnAction;
    public string SearchController => Global.PPTextSearchController;

    public PupilPremiumLearnerTextSearchController(
        ILogger<PupilPremiumLearnerTextSearchController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        ITextSearchSelectionManager selectionManager,
        ISessionProvider sessionProvider,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupils,
        IUseCase<PupilPremiumSearchRequest, PupilPremiumSearchResponse> searchUseCase,
        IMapper<PupilPremiumLearnerTextSearchMappingContext, LearnerTextSearchViewModel> learnerSearchResponseToViewModelMapper,
        IMapper<Dictionary<string, string[]>, IList<FilterRequest>> filtersRequestMapper,
        IMapper<SortOrderRequest, SortOrder> sortOrderViewModelToRequestMapper,
        IFiltersRequestFactory filtersRequestBuilder,
        ISearchCriteriaProvider searchCriteriaProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

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

        ArgumentNullException.ThrowIfNull(searchUseCase);
        _searchUseCase = searchUseCase;

        ArgumentNullException.ThrowIfNull(learnerSearchResponseToViewModelMapper);
        _learnerSearchResponseToViewModelMapper = learnerSearchResponseToViewModelMapper;

        ArgumentNullException.ThrowIfNull(filtersRequestBuilder);
        _filtersRequestMapper = filtersRequestMapper;

        ArgumentNullException.ThrowIfNull(sortOrderViewModelToRequestMapper);
        _sortOrderViewModelToRequestMapper = sortOrderViewModelToRequestMapper;

        ArgumentNullException.ThrowIfNull(filtersRequestBuilder);
        _filtersRequestBuilder = filtersRequestBuilder;

        ArgumentNullException.ThrowIfNull(searchCriteriaProvider);
        _searchCriteriaProvider = searchCriteriaProvider;
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
        [FromQuery] string sortDirection)
    {
        _logger.LogInformation("Pupil Premium NonUpn POST method called");
        model.ShowHiddenUPNWarningMessage = true;
        return await Search(
            model,
            surnameFilter,
            middleNameFilter,
            forenameFilter,
            searchByRemove,
            model.PageNumber,
            sortField,
            sortDirection,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [Route(Routes.PupilPremium.NonUpnDobFilter)]
    [HttpPost]
    public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
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

        return await ReturnToRoute(model);
    }

    [Route(Routes.PupilPremium.NonUPNSurnameFilter)]
    [HttpPost]
    public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
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

        return await ReturnToRoute(model);
    }

    [Route(Routes.PupilPremium.NonUpnMiddlenameFilter)]
    [HttpPost]
    public async Task<IActionResult> MiddlenameFilter(LearnerTextSearchViewModel model, string middlenameFilter)
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

        return await ReturnToRoute(model);
    }

    [Route(Routes.PupilPremium.NonUpnForenameFilter)]
    [HttpPost]
    public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
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

        return await ReturnToRoute(model);
    }

    [Route(Routes.PupilPremium.NonUpnSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        SetPersistedSexFiltersForViewModel(model);
        ModelState.Clear();
        return await ReturnToRoute(model);
    }

    [HttpPost]
    [Route("add-pp-nonupn-to-my-pupil-list")]
    public async Task<IActionResult> PPAddToMyPupilList(LearnerTextSearchViewModel model)
    {
        PopulatePageText(model);
        PopulateNavigation(model);
        SetSortOptions(model);

        SetSelections(model.SelectedPupil);

        string selectedUpn = _selectionManager.GetSelectedFromSession();

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

        string selectedPupil = _selectionManager.GetSelectedFromSession();

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

        LearnerDownloadViewModel searchDownloadViewModel = new()
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
            LearnerDownloadViewModel searchDownloadViewModel = new()
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
    public async Task<IActionResult> DownloadCancellationReturn()
    {
        return await Search(true);
    }

    #region WIP legacy inherited methods to deprecate
    private void PopulateConfirmationNavigation(StarredPupilConfirmationViewModel model)
    {
        model.DownloadType = DownloadType.PupilPremium;
        model.ConfirmationReturnController = SearchController;
        model.ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction;
        model.CancelReturnController = SearchController;
        model.CancelReturnAction = Global.PPDownloadCancellationReturnAction;
    }

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

            model = await GenerateLearnerTextSearchViewModel(
                model,
                null,
                null,
                null,
                null,
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
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove,
        int pageNumber,
        string sortField = "",
        string sortDirection = "",
        bool resetSelection = false)
    {
        GetPersistedSexFiltersForViewModel(model);
        model.SearchText = SecurityHelper.SanitizeText(model.SearchText);
        model.LearnerNumberLabel = LearnerNumberLabel;
        
        if (resetSelection || searchByRemove != null)
        {
            _selectionManager.Clear();
            ClearSortOptions();

            if (!string.IsNullOrWhiteSpace(searchByRemove))
            {
                model.SelectedSexValues =
                    model.SelectedSexValues?.Where(selectedSexValue =>
                        selectedSexValue.SwitchSexToParseName() != searchByRemove).ToArray();
            }

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
            _sessionProvider.SetSessionValue(SortFieldKey, sortField);
            _sessionProvider.SetSessionValue(SortDirectionKey, sortDirection);
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

        model.ReturnRoute = ReturnRoute.NonPupilPremium;

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

        return await Search(
            model,
            null,
            null,
            null,
            null,
            model.PageNumber,
            sortField: model.SortField,
            sortDirection: model.SortDirection);
    }

    private async Task<IActionResult> ReturnToRoute(LearnerTextSearchViewModel model)
    {
        _selectionManager.Clear();
        ClearSortOptions();

        return await Search(model, null, null, null, null, model.PageNumber);
    }

    private void GetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model)
    {
        string[] sexFilters =
            TempData.GetPersistedObject<string[]>(
                PersistedSelectedSexFiltersKey,
                keepTempDataBetweenRequests: true);

        if (sexFilters != null)
            model.SelectedSexValues = sexFilters;
    }

    private void SetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model) =>
            TempData.SetPersistedObject(
                model.SelectedSexValues,
                PersistedSelectedSexFiltersKey);

    private void RemoveAllSexFilterItemsFromModel(
        LearnerTextSearchViewModel model)
    {
        model.SelectedSexValues = null;
        SetPersistedSexFiltersForViewModel(model);
        TempData.Remove(PersistedSelectedSexFiltersKey);
    }

    private void SetSortOptions(LearnerTextSearchViewModel model)
    {
        if (HttpContext.Session.Keys.Contains(SortDirectionKey))
        {
            model.SortDirection = HttpContext.Session.GetString(SortDirectionKey);
        }
            
        if (HttpContext.Session.Keys.Contains(SortFieldKey))
        {
            model.SortField = HttpContext.Session.GetString(SortFieldKey);
        }
    }

    private void SetSearchFiltersUrls(LearnerTextSearchViewModel model)
    {
        model.RedirectUrls.SurnameFilterURL = Routes.PupilPremium.NonUPNSurnameFilter;
        model.RedirectUrls.FormAction = Routes.PupilPremium.NonUPN;
        model.RedirectUrls.RemoveAction = $"/{Routes.Application.Search}/{Routes.PupilPremium.NonUPN}";
        model.RedirectUrls.DobFilterUrl = DobFilterUrl;
        model.RedirectUrls.ForenameFilterUrl = Routes.PupilPremium.NonUpnForenameFilter;
        model.RedirectUrls.MiddlenameFilterUrl = Routes.PupilPremium.NonUpnMiddlenameFilter;
        model.RedirectUrls.SexFilterUrl = Routes.PupilPremium.NonUpnSexFilter;
    }

    private void ClearSortOptions()
    {
        HttpContext.Session.Remove(SortDirectionKey);
        HttpContext.Session.Remove(SortFieldKey);
    }

    private void PopulateNavigation(LearnerTextSearchViewModel model)
    {
        model.LearnerTextSearchController = SearchController;
        model.LearnerTextSearchAction = SearchAction;

        model.LearnerNumberController = Routes.Application.Search;
        model.LearnerNumberAction = SearchLearnerNumberAction;
        model.RedirectUrls.FormAction = Global.PPNonUpnAction;

        model.LearnerTextDatabaseName = LearnerTextDatabaseName;
        model.RedirectUrls.DobFilterUrl = DobFilterUrl;
        model.RedirectFrom = Routes.PupilPremium.NonUPN;
        model.DownloadLinksPartial = DownloadLinksPartial;
    }

    private void SetSelections(string selected)
    {
        if (!string.IsNullOrEmpty(selected))
        {
            _selectionManager.Clear();
            _selectionManager.Add(selected);
        }
    }


    private IActionResult ConfirmationForStarredPupil(StarredPupilConfirmationViewModel model)
    {
        LearnerTextSearchViewModel searchViewModel = new()
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

    private async Task<LearnerTextSearchViewModel> GenerateLearnerTextSearchViewModel(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string foremameFilter,
        string searchByRemove,
        string sortField = "",
        string sortDirection = "")
    {
        List<CurrentFilterDetail> currentFilters =
            SetCurrentFilters(model, surnameFilter, middlenameFilter, foremameFilter, searchByRemove);

        IList<FilterRequest> filterRequests =
           _filtersRequestMapper.Map(
               _filtersRequestBuilder
                   .GenerateFilterRequest(model, currentFilters));

        SortOrder sortOrder =
            _sortOrderViewModelToRequestMapper.Map(
                new SortOrderRequest(
                    searchKey: "pupil-premium",
                    sortOrder: (sortField, sortDirection)));

        SearchCriteria searchCriteria = _searchCriteriaProvider.GetCriteria("pupil-premium-text");

        PupilPremiumSearchResponse searchResponse =
            await _searchUseCase.HandleRequestAsync(
                new PupilPremiumSearchRequest(
                    searchKeywords: model.SearchText,
                    filterRequests: filterRequests,
                    searchCriteria: searchCriteria,
                    sortOrder: sortOrder,
                    offset: model.Offset));

        LearnerTextSearchViewModel viewModel = _learnerSearchResponseToViewModelMapper.Map(
            PupilPremiumLearnerTextSearchMappingContext.Create(model, searchResponse));

        SetLearnerNumberId(viewModel);

        if (!User.IsAdmin())
        {
            viewModel.Learners = RbacHelper.CheckRbacRulesGeneric(
                viewModel.Learners.ToList(),
                statutoryLowAge: User.GetOrganisationLowAge(),
                statutoryHighAge: User.GetOrganisationHighAge());
        }

        string selected = _selectionManager.GetSelectedFromSession();

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

        viewModel.LearnerTextDatabaseName = LearnerTextDatabaseName;

        SetSearchFiltersUrls(viewModel);

        viewModel.PageLearnerNumbers = string.Join(',', viewModel.Learners.Select(l => l.LearnerNumberId));

        viewModel.ShowOverLimitMessage = viewModel.Total > 100000;

        if (ModelState.IsValid)
        {
            viewModel.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
            viewModel.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
            viewModel.DownloadSelectedLink = ApplicationLabels.DownloadSelectedPupilPremiumDataLink;

            if (currentFilters.Count > 0)
            {
                viewModel.SearchFilters.CurrentFiltersApplied = currentFilters;
            }

            viewModel.SearchFilters.CurrentFiltersAppliedString = JsonSerializer.Serialize(currentFilters);
        }

        return viewModel;
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

        private static void PopulatePageText(LearnerTextSearchViewModel model)
    {
        model.PageHeading = ApplicationLabels.SearchPupilPremiumWithOutUpnPageHeading;
        model.ShowMiddleNames = true;
        model.ShowLocalAuthority = true;
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


    private static List<CurrentFilterDetail> SetCurrentFilters(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove)
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
