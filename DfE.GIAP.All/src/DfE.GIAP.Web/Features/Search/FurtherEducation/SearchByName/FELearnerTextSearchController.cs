using System.Text.Json;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Search.Options.Search;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.Controllers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using static DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName.FurtherEducationLearnerTextSearchResponseToViewModelMapper;


namespace DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;

[Route(Routes.Application.Search)]
public class FELearnerTextSearchController : Controller
{
    private const bool ShowMiddleNames = false;
    private const int PAGESIZE = 20;
    private const string PersistedSelectedSexFiltersKey = "PersistedSelectedSexFilters";
    private const string PageHeading = ApplicationLabels.SearchFEWithoutUlnPageHeading;
    private const string LearnerNumberLabel = Global.FELearnerNumberLabel;

    public string SearchSessionKey => Global.FENonUlnSearchSessionKey;
    public string SearchFiltersSessionKey => Global.FENonUlnSearchFiltersSessionKey;

    public string SortDirectionKey => Global.FENonUlnSortDirectionSessionKey;
    public string SortFieldKey => Global.FENonUlnSortFieldSessionKey;

    private readonly ISessionProvider _sessionProvider;
    private readonly IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> _getAvailableDatasetsForPupilsUseCase;
    private readonly IEventLogger _eventLogger;
    private readonly ILogger<FELearnerTextSearchController> _logger;
    protected readonly ITextSearchSelectionManager _selectionManager;
    private readonly IUseCase<
        FurtherEducationSearchByNameRequest,
        FurtherEducationSearchByNameResponse> _furtherEducationSearchUseCase;

    private readonly IMapper<
        FurtherEducationLearnerTextSearchMappingContext,
        LearnerTextSearchViewModel> _learnerSearchResponseToViewModelMapper;

    private readonly IMapper<
        Dictionary<string, string[]>,
        IList<FilterRequest>> _filtersRequestMapper;

    private readonly ISortOrderFactory _sortOrderFactory;

    private readonly IMapper<SearchCriteriaOptions, SearchCriteria> _criteriaOptionsToCriteriaMapper;

    private readonly IFiltersRequestFactory _filtersRequestFactory;
    
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadPupilDataUseCase;

    private readonly ISearchIndexOptionsProvider _searchIndexOptionsProvider;

    public FELearnerTextSearchController(
        ISessionProvider sessionProvider,
        IUseCase<
            FurtherEducationSearchByNameRequest,
            FurtherEducationSearchByNameResponse> furtherEducationSearchUseCase,
        IMapper<
            FurtherEducationLearnerTextSearchMappingContext,
            LearnerTextSearchViewModel> learnerSearchResponseToViewModelMapper,
        IMapper<
            Dictionary<string, string[]>,
            IList<FilterRequest>> filtersRequestMapper,
        ISortOrderFactory sortOrderFactory,
        IFiltersRequestFactory filtersRequestBuilder,
        ILogger<FELearnerTextSearchController> logger,
        ITextSearchSelectionManager selectionManager,
        IEventLogger eventLogger,
        IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> getAvailableDatasetsForPupilsUseCase,
        IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> downloadPupilDataUseCase,
        ISearchIndexOptionsProvider searchIndexOptionsProvider,
        IMapper<SearchCriteriaOptions, SearchCriteria> criteriaOptionsToCriteriaMapper)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(furtherEducationSearchUseCase);
        _furtherEducationSearchUseCase = furtherEducationSearchUseCase;

        ArgumentNullException.ThrowIfNull(learnerSearchResponseToViewModelMapper);
        _learnerSearchResponseToViewModelMapper = learnerSearchResponseToViewModelMapper;

        ArgumentNullException.ThrowIfNull(filtersRequestMapper);
        _filtersRequestMapper = filtersRequestMapper;

        ArgumentNullException.ThrowIfNull(sortOrderFactory);
        _sortOrderFactory = sortOrderFactory;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(filtersRequestBuilder);
        _filtersRequestFactory = filtersRequestBuilder;

        ArgumentNullException.ThrowIfNull(getAvailableDatasetsForPupilsUseCase);
        _getAvailableDatasetsForPupilsUseCase = getAvailableDatasetsForPupilsUseCase;

        ArgumentNullException.ThrowIfNull(eventLogger);
        _eventLogger = eventLogger;

        ArgumentNullException.ThrowIfNull(downloadPupilDataUseCase);
        _downloadPupilDataUseCase = downloadPupilDataUseCase;

        ArgumentNullException.ThrowIfNull(searchIndexOptionsProvider);
        _searchIndexOptionsProvider = searchIndexOptionsProvider;

        ArgumentNullException.ThrowIfNull(criteriaOptionsToCriteriaMapper);
        _criteriaOptionsToCriteriaMapper = criteriaOptionsToCriteriaMapper;
    }

    private bool HasAccessToFurtherEducationSearch =>
        User.IsAdmin() ||
            User.IsEstablishmentWithFurtherEducation() ||
                User.IsEstablishmentWithAccessToULNPages() ||
                    User.IsDfeUser();

    [Route(Routes.FurtherEducation.LearnerTextSearch)]
    [HttpGet]
    public async Task<IActionResult> FurtherEducationNonUlnSearch(bool? returnToSearch)
    {
        if (!HasAccessToFurtherEducationSearch)
        {
            return RedirectToAction("Error", "Home");
        }

        _logger.LogInformation("Further education non ULN search GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.FurtherEducation.LearnerTextSearch)]
    [HttpPost]
    public async Task<IActionResult> FurtherEducationNonUlnSearch(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("Further education non ULN search POST method called");
        model.ShowHiddenUPNWarningMessage = false;

        return await Search(
            model,
            surnameFilter,
            middlenameFilter,
            forenameFilter,
            searchByRemove,
            model.PageNumber,
            sortField,
            sortDirection,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [Route(Routes.FurtherEducation.NonULNDobFilter)]
    [HttpPost]
    public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
    {
        return await DobSearchFilter(model);
    }

    [Route(Routes.FurtherEducation.NonULNSurnameFilter)]
    [HttpPost]
    public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
    {
        return await SurnameSearchFilter(model, surnameFilter);
    }

    [Route(Routes.FurtherEducation.NonULNForenameFilter)]
    [HttpPost]
    public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
    {
        return await ForenameSearchFilter(model, forenameFilter);
    }

    [Route(Routes.FurtherEducation.NonULNSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        SetPersistedSexFiltersForViewModel(model);
        ModelState.Clear();
        return await ReturnToRoute(model);
    }

    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedFurtherEducationData(
        string selectedPupil,
        string searchText)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupil,
            LearnerNumber = selectedPupil,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = false
        };

        GetAvailableDatasetsForPupilsRequest request = new(
            DownloadType: Core.Downloads.Application.Enums.DownloadType.FurtherEducation,
            SelectedPupils: [selectedPupil],
            AuthorisationContext: new HttpClaimsAuthorisationContext(User));

        GetAvailableDatasetsForPupilsResponse response = await _getAvailableDatasetsForPupilsUseCase.HandleRequestAsync(request);

        foreach (AvailableDatasetResult datasetResult in response.AvailableDatasets)
        {
            searchDownloadViewModel.SearchDownloadDatatypes.Add(new SearchDownloadDataType
            {
                Name = StringHelper.StringValueOfEnum(datasetResult.Dataset),
                Value = datasetResult.Dataset.ToString(),
                Disabled = !datasetResult.CanDownload || !datasetResult.HasData,
            });
        }

        ModelState.Clear();

        searchDownloadViewModel.TextSearchViewModel.PageLearnerNumbers = selectedPupil;
        searchDownloadViewModel.SearchAction = Global.FELearnerTextSearchAction;
        searchDownloadViewModel.DownloadRoute = Routes.FurtherEducation.DownloadNonUlnFile;
        searchDownloadViewModel.RedirectRoute = Routes.FurtherEducation.LearnerTextSearch;
        searchDownloadViewModel.TextSearchViewModel = new LearnerTextSearchViewModel()
        {
            LearnerNumberLabel = LearnerNumberLabel,
            SearchText = searchText
        };
        PopulateNavigation(searchDownloadViewModel.TextSearchViewModel);

        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.NonLearnerNumberDownloadOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.FurtherEducation.DownloadNonUlnFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadFurtherEducationFile(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.LearnerNumber))
        {
            string[] selectedPupils = model.LearnerNumber.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType is not DownloadFileType.None)
            {
                // Parse requested datasets
                List<Core.Downloads.Application.Enums.Dataset> selectedDatasets = new();
                foreach (string datasetString in model.SelectedDownloadOptions)
                {
                    if (Enum.TryParse(datasetString, ignoreCase: true, out Core.Downloads.Application.Enums.Dataset dataset))
                        selectedDatasets.Add(dataset);
                }

                DownloadPupilDataRequest request = new(
                    SelectedPupils: selectedPupils,
                    SelectedDatasets: selectedDatasets,
                    DownloadType: Core.Downloads.Application.Enums.DownloadType.FurtherEducation,
                    FileFormat: FileFormat.Csv);

                DownloadPupilDataResponse response = await _downloadPupilDataUseCase.HandleRequestAsync(request);

                string loggingBatchId = Guid.NewGuid().ToString();
                foreach (string dataset in model.SelectedDownloadOptions)
                {
                    // TODO: Temp quick solution
                    if (Enum.TryParse(dataset, out Core.Common.CrossCutting.Logging.Events.Dataset datasetEnum))
                    {
                        _eventLogger.LogDownload(
                            Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                            DownloadFileFormat.CSV,
                            DownloadEventType.FE,
                            loggingBatchId,
                            datasetEnum);
                    }
                }

                if (response.FileContents is not null)
                {
                    model.ErrorDetails = null;
                    return File(response.FileContents, response.ContentType, response.FileName);
                }
                else
                {
                    model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
                }
            }
            else
            {
                model.ErrorDetails = Messages.Search.Errors.SelectFileType;
            }

            TempData["ErrorDetails"] = model.ErrorDetails;

            if (_sessionProvider.ContainsSessionKey(SearchSessionKey))
                model.TextSearchViewModel.SearchText = _sessionProvider.GetSessionValue(SearchSessionKey);

            return await DownloadSelectedFurtherEducationData(model.SelectedPupils, model.TextSearchViewModel?.SearchText);
        }

        return RedirectToAction(Global.FELearnerTextSearchAction, Global.FELearnerTextSearchController);
    }

    [Route(Routes.FurtherEducation.DownloadNonUlnRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedFEDataULN(LearnerTextSearchViewModel model)
    {
        SetSelections(model.SelectedPupil);

        string selectedPupil = GetSelected();

        if (string.IsNullOrEmpty(selectedPupil))
        {
            model.NoPupil = true;
            model.NoPupilSelected = true;
            model.ErrorDetails = Messages.Downloads.Errors.NoLearnerSelected;
            return await FurtherEducationNonUlnSearch(model, null, null, null, null, null, null);
        }

        return await DownloadSelectedFurtherEducationData(selectedPupil, model.SearchText);
    }

    #region WIP - this will be slowly refactored away from the controller as we move through more search types

    [NonAction]
    public async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerTextSearchViewModel model = new()
        {
            LearnerNumberLabel = LearnerNumberLabel,
            ShowMiddleNames = ShowMiddleNames
        };

        PopulatePageText(model);
        PopulateNavigation(model);

        if (returnToSearch ?? false)
        {
            if (_sessionProvider.ContainsSessionKey(SearchSessionKey))
                model.SearchText = _sessionProvider.GetSessionValue(SearchSessionKey);

            if (_sessionProvider.ContainsSessionKey(SearchFiltersSessionKey))
                model.SearchFilters =
                    _sessionProvider.GetSessionValueOrDefault<SearchFilters>(SearchFiltersSessionKey);

            SetSortOptions(model);
            GetPersistedSexFiltersForViewModel(model);
            model.PageNumber = 0;
            model.PageSize = PAGESIZE;
            model = await GenerateLearnerTextSearchViewModel(model, null, null, null, null, model.SortField, model.SortDirection);
        }

        if (!returnToSearch.HasValue)
        {
            _selectionManager.Clear();
        }

        return View(Global.NonUpnSearchView, model);
    }

    [NonAction]
    public async Task<IActionResult> Search(
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

        model.ReturnRoute = ReturnRoute.NonUniqueLearnerNumber;

        PopulatePageText(model);
        PopulateNavigation(model);

        model.ShowMiddleNames = ShowMiddleNames;

        _sessionProvider.SetSessionValue(SearchSessionKey, model.SearchText);

        if (model.SearchFilters != null)
        {
            _sessionProvider.SetSessionValue(SearchFiltersSessionKey, model.SearchFilters);
        }

        return View(Global.NonUpnSearchView, model);
    }


    [NonAction]
    public async Task<IActionResult> DobSearchFilter(LearnerTextSearchViewModel model)
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

        if (!model.FilterErrors.DobError && day > 0 && month > 0 && year > 0 && !PupilHelper.IsValidateDate($"{day.ToString("00")}/{month.ToString("00")}/{year}"))
        {
            ModelState.AddModelError("InvalidDate", Messages.Search.Errors.DobInvalid);
            model.FilterErrors.InvalidDob = true;
            model.FilterErrors.DobError = true;
        }

        return await ReturnToRoute(model).ConfigureAwait(false);
    }


    [NonAction]
    public async Task<IActionResult> SurnameSearchFilter(LearnerTextSearchViewModel model, string surnameFilter)
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

    [NonAction]
    public async Task<IActionResult> MiddlenameSearchFilter(LearnerTextSearchViewModel model, string middlenameFilter)
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

    [NonAction]
    public async Task<IActionResult> ForenameSearchFilter(LearnerTextSearchViewModel model, string forenameFilter)
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

    private void RemoveSexFilterItemFromModel(
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

    private void RemoveAllSexFilterItemsFromModel(
        LearnerTextSearchViewModel model)
    {
        model.SelectedSexValues = null;
        SetPersistedSexFiltersForViewModel(model);
        TempData.Remove(PersistedSelectedSexFiltersKey);
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


        bool isCustomSearch = !string.IsNullOrWhiteSpace(model.SearchText);
        Dictionary<string, bool> flags = ConvertFiltersToFlags(currentFilters);
        _eventLogger.LogSearch(SearchIdentifierType.ULN, isCustomSearch, flags);

        SetLearnerTextAction(model);
        model.ShowMiddleNames = ShowMiddleNames;

        SetSearchFiltersUrls(model);

        if (ModelState.IsValid)
        {
            model.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
            model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
            model.DownloadSelectedLink = ApplicationLabels.DownloadSelectedFurtherEducationLink;

            if (currentFilters.Count > 0)
            {
                model.SearchFilters.CurrentFiltersApplied = currentFilters;
            }

            model.SearchFilters.CurrentFiltersAppliedString = JsonSerializer.Serialize(currentFilters);

        }

        SearchIndexOptions indexOptions = _searchIndexOptionsProvider.GetOptions(key: "further-education-text");

        IList<FilterRequest> filterRequests =
            _filtersRequestMapper.Map(
                _filtersRequestFactory
                    .GenerateFilterRequest(model, currentFilters));

        SortOrder sortOrder =
            _sortOrderFactory.Create(
                options: indexOptions.SortOptions, 
                sort: (sortField, sortDirection));

        
        FurtherEducationSearchByNameResponse searchResponse =
            await _furtherEducationSearchUseCase.HandleRequestAsync(
                new FurtherEducationSearchByNameRequest()
                {
                    SearchKeywords = model.SearchText,
                    FilterRequests = filterRequests,
                    SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(indexOptions.SearchCriteria),
                    SortOrder = sortOrder,
                    Offset = model.Offset
                });

        return _learnerSearchResponseToViewModelMapper.Map(
            FurtherEducationLearnerTextSearchMappingContext.Create(model, searchResponse));
    }

    public static Dictionary<string, bool> ConvertFiltersToFlags(List<CurrentFilterDetail> filters)
    {
        Dictionary<string, bool> flags = new();

        if (filters is null)
            return flags;

        foreach (CurrentFilterDetail filerDetails in filters)
        {
            // True if the filter has at least one value
            flags[filerDetails.FilterType.ToString()] = true;
        }

        return flags;
    }

    private static List<CurrentFilterDetail> SetCurrentFilters(LearnerTextSearchViewModel model,
       string surnameFilter, string middlenameFilter, string forenameFilter, string searchByRemove)
    {
        List<CurrentFilterDetail> currentFilters =
            !string.IsNullOrEmpty(model.SearchFilters.CurrentFiltersAppliedString)
                ? JsonSerializer.Deserialize<List<CurrentFilterDetail>>(model.SearchFilters.CurrentFiltersAppliedString) : [];

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
                    FilterName = filterValue.ToLowerInvariant().Trim()
                }
            );
        }
    }

    private static List<CurrentFilterDetail> RemoveFilterValue(string searchByRemove, List<CurrentFilterDetail> currentFilters, LearnerTextSearchViewModel model)
    {
        if (!string.IsNullOrEmpty(searchByRemove))
        {
            CurrentFilterDetail item = currentFilters.Find(x => x.FilterName == searchByRemove);
            if (item != null)
            {
                currentFilters.Remove(item);
            }

            CurrentFilterDetail sexFiltersActive = currentFilters.Find(x => x.FilterType == FilterType.Sex);
            if (sexFiltersActive != null && model.SelectedSexValues == null && currentFilters.Count() >= 1)
            {
                List<string> currentSelectedSexList = new List<string>();
                foreach (CurrentFilterDetail filter in currentFilters)
                {
                    currentSelectedSexList.Add(filter.FilterName.Substring(0, 1));
                }
                model.SelectedSexValues = currentSelectedSexList.ToArray();
            }
        }
        return currentFilters;
    }

    private static void SetSearchFiltersUrls(LearnerTextSearchViewModel model)
    {
        model.RedirectUrls.SurnameFilterURL = Routes.FurtherEducation.NonULNSurnameFilter;
        model.RedirectUrls.FormAction = Routes.FurtherEducation.LearnerTextSearch;
        model.RedirectUrls.RemoveAction = $"/{Routes.Application.Search}/{Routes.FurtherEducation.LearnerTextSearch}";
        SetDateOfBirthFilterUrl(model);
        model.RedirectUrls.ForenameFilterUrl = Routes.FurtherEducation.NonULNForenameFilter;
        model.RedirectUrls.MiddlenameFilterUrl = string.Empty;
        model.RedirectUrls.SexFilterUrl = Routes.FurtherEducation.NonULNSexFilter;
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

        currentFiltersSex.ToList().ForEach(sex =>
            currentFilters = RemoveFilterValue(sex, currentFilters, model));
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

    private void SetSortOptions(LearnerTextSearchViewModel model)
    {
        if (_sessionProvider.ContainsSessionKey(SortDirectionKey))
            model.SortDirection = _sessionProvider.GetSessionValue(SortDirectionKey);

        if (_sessionProvider.ContainsSessionKey(SortFieldKey))
            model.SortField = _sessionProvider.GetSessionValue(SortFieldKey);
    }

    private void ClearSortOptions()
    {
        _sessionProvider.RemoveSessionValue(SortDirectionKey);
        _sessionProvider.RemoveSessionValue(SortFieldKey);
    }

    private LearnerTextSearchViewModel PopulatePageText(LearnerTextSearchViewModel model)
    {
        model.PageHeading = PageHeading;
        model.ShowLocalAuthority = false;
        return model;
    }

    private LearnerTextSearchViewModel PopulateNavigation(LearnerTextSearchViewModel model)
    {
        model.LearnerTextSearchController = Global.FELearnerTextSearchController;
        model.LearnerTextSearchAction = Global.FELearnerTextSearchAction;

        model.LearnerNumberController = Routes.Application.Search;
        model.LearnerNumberAction = Routes.FurtherEducation.LearnerNumberSearch;
        model.RedirectUrls.FormAction = Global.FELearnerTextSearchAction;

        SetLearnerTextAction(model);
        SetDateOfBirthFilterUrl(model);
        model.RedirectFrom = Routes.FurtherEducation.LearnerTextSearch;
        model.DownloadLinksPartial = Global.FENonUlnDownloadLinksView;

        return model;
    }

    private static void SetDateOfBirthFilterUrl(LearnerTextSearchViewModel model)
    {
        model.RedirectUrls.DobFilterUrl = Routes.FurtherEducation.NonULNDobFilter;
    }

    private static void SetLearnerTextAction(LearnerTextSearchViewModel model)
    {
        model.LearnerTextDatabaseName = Global.FELearnerTextSearchAction;
    }

    private void SetSelections(string selected)
    {
        if (!string.IsNullOrEmpty(selected))
        {
            _selectionManager.Clear();
            _selectionManager.Add(selected);
        }
    }

    private string GetSelected() => _selectionManager.GetSelectedFromSession();

    #endregion
}
