using System.Text.Json;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Controllers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers.LearnerSearchResponseToViewModelMapper;


namespace DfE.GIAP.Web.Controllers.TextBasedSearch;

[Route(Routes.Application.Search)]
public class FELearnerTextSearchController :  Controller
{
    public const int PAGESIZE = 20;
    private const string PersistedSelectedGenderFiltersKey = "PersistedSelectedGenderFilters";
    private const string PersistedSelectedSexFiltersKey = "PersistedSelectedSexFilters";

    #region This lot needs sorting - massive bleed of responsibilities and no encapsulation of concerns, this is a controller not a service!

    // TODO: this needs sorting cos some amateur wrote this nonsense!
    public string PageHeading => ApplicationLabels.SearchFEWithoutUlnPageHeading;
    public  string SearchSessionKey => Global.FENonUlnSearchSessionKey;
    public  string SearchFiltersSessionKey => Global.FENonUlnSearchFiltersSessionKey;
    public  string SortDirectionKey => Global.FENonUlnSortDirectionSessionKey;
    public  string SortFieldKey => Global.FENonUlnSortFieldSessionKey;
    public  string DownloadLinksPartial => Global.FENonUlnDownloadLinksView;
    public  string SearchLearnerNumberAction => Routes.FurtherEducation.LearnerNumberSearch;
    public  string RedirectUrlFormAction => Global.FELearnerTextSearchAction;
    public  string LearnerTextDatabaseAction => Global.FELearnerTextSearchAction;
    public  string LearnerTextDatabaseName => Global.FELearnerTextSearchAction;
    public  string RedirectFrom => Routes.FurtherEducation.LearnerTextSearch;

    public  string SurnameFilterUrl => Routes.FurtherEducation.NonULNSurnameFilter;
    public  string DobFilterUrl => Routes.FurtherEducation.NonULNDobFilter;
    public  string ForenameFilterUrl => Routes.FurtherEducation.NonULNForenameFilter;
    public  string MiddlenameFilterUrl => "";
    public  string GenderFilterUrl => Routes.FurtherEducation.NonULNGenderFilter;
    public  string SexFilterUrl => Routes.FurtherEducation.NonULNSexFilter;

    public  string FormAction => Routes.FurtherEducation.LearnerTextSearch;
    public  string RemoveActionUrl => $"/{Routes.Application.Search}/{Routes.FurtherEducation.LearnerTextSearch}";
    public  AzureSearchIndexType IndexType => AzureSearchIndexType.FurtherEducation;
    public  string SearchView => Global.NonUpnSearchView;

    public  string SearchLearnerNumberController => Routes.Application.Search;
    public  int MyPupilListLimit => _appSettings.NonUpnNPDMyPupilListLimit; //Not valid for FE so arbitrarily set to default non UPN limit
    public  string SearchAction => Global.FELearnerTextSearchAction;
    public  string SearchController => Global.FELearnerTextSearchController;
    public  ReturnRoute ReturnRoute => ReturnRoute.NonUniqueLearnerNumber;
    public  string LearnerTextSearchController => Global.FELearnerTextSearchController;
    public  string LearnerTextSearchAction => Global.FELearnerTextSearchAction;
    public  string LearnerNumberAction => Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber;

    public bool ShowGender => true; //_appSettings.FeUseGender;
    public  bool ShowLocalAuthority => false;
    public  string InvalidUPNsConfirmationAction => "";
    public  string LearnerNumberLabel => Global.FELearnerNumberLabel;
    public  bool ShowMiddleNames => false;

    public  string DownloadSelectedLink => ApplicationLabels.DownloadSelectedFurtherEducationLink;

    #endregion

    private readonly ISessionProvider _sessionProvider;
    private readonly IDownloadService _downloadService;
    private readonly ILogger<FELearnerTextSearchController> _logger;
    protected readonly ITextSearchSelectionManager _selectionManager;
    private readonly AzureAppSettings _appSettings;
    private readonly IUseCase<
        SearchByKeyWordsRequest,
        SearchByKeyWordsResponse> _furtherEducationSearchUseCase;

    private readonly IMapper<
        LearnerSearchMappingContext,
        LearnerTextSearchViewModel> _learnerSearchResponseToViewModelMapper;

    private readonly IMapper<
        Dictionary<string, string[]>,
        IList<FilterRequest>> _filtersRequestMapper;

    private readonly IMapper<
        (string Field, string Direction), SortOrder> _sortOrderViewModelToRequestMapper;

    private readonly IFiltersRequestFactory _filtersRequestBuilder;

    public FELearnerTextSearchController(
        ISessionProvider sessionProvider,
        IUseCase<
            SearchByKeyWordsRequest,
            SearchByKeyWordsResponse> furtherEducationSearchUseCase,
        IMapper<
            LearnerSearchMappingContext,
            LearnerTextSearchViewModel> learnerSearchResponseToViewModelMapper,
        IMapper<
            Dictionary<string, string[]>,
            IList<FilterRequest>> filtersRequestMapper,
        IMapper<
            (string Field, string Direction), SortOrder> sortOrderViewModelToRequestMapper,
        IFiltersRequestFactory filtersRequestBuilder,
        ILogger<FELearnerTextSearchController> logger,
           IPaginatedSearchService paginatedSearch,
           ITextSearchSelectionManager selectionManager,
           IDownloadService downloadService,
           IOptions<AzureAppSettings> azureAppSettings)
    {
        _sessionProvider = sessionProvider ??
            throw new ArgumentNullException(nameof(sessionProvider));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
        _appSettings = azureAppSettings.Value;

        _furtherEducationSearchUseCase = furtherEducationSearchUseCase ??
            throw new ArgumentNullException(nameof(furtherEducationSearchUseCase));
        _learnerSearchResponseToViewModelMapper = learnerSearchResponseToViewModelMapper ??
            throw new ArgumentNullException(nameof(learnerSearchResponseToViewModelMapper));
        _filtersRequestMapper = filtersRequestMapper ??
            throw new ArgumentNullException(nameof(filtersRequestMapper));
        _sortOrderViewModelToRequestMapper = sortOrderViewModelToRequestMapper ??
            throw new ArgumentNullException(nameof(sortOrderViewModelToRequestMapper));

        _selectionManager = selectionManager ??
            throw new ArgumentNullException(nameof(selectionManager));
        _appSettings = azureAppSettings.Value;
        _filtersRequestBuilder = filtersRequestBuilder ??
            throw new ArgumentNullException(nameof(filtersRequestBuilder));
    }


    [Route(Routes.FurtherEducation.LearnerTextSearch)]
    [HttpGet]
    public async Task<IActionResult> FurtherEducationNonUlnSearch(bool? returnToSearch)
    {
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
            model, surnameFilter, middlenameFilter, forenameFilter,
            searchByRemove, model.PageNumber,
            ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController, sortField, sortDirection,
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

    [Route(Routes.FurtherEducation.NonULNGenderFilter)]
    [HttpPost]
    public async Task<IActionResult> GenderFilter(LearnerTextSearchViewModel model)
    {
        return await GenderSearchFilter(model);
    }

    [Route(Routes.FurtherEducation.NonULNSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        return await SexSearchFilter(model);
    }

    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedFurtherEducationData(
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
            ShowTABDownloadType = false
        };

        if (IndexType == AzureSearchIndexType.FurtherEducation)
        {
            SearchDownloadHelper.AddUlnDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationHighAge(), User.IsDfeUser());
        }
        else
        {
            SearchDownloadHelper.AddDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationLowAge(), User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(), User.IsOrganisationAllAges());
        }

        ModelState.Clear();

        searchDownloadViewModel.TextSearchViewModel.PageLearnerNumbers = selectedPupil;
        searchDownloadViewModel.SearchAction = Global.FELearnerTextSearchAction;
        searchDownloadViewModel.DownloadRoute = Routes.FurtherEducation.DownloadNonUlnFile;
        searchDownloadViewModel.RedirectRoute = Routes.FurtherEducation.LearnerTextSearch;
        searchDownloadViewModel.TextSearchViewModel = new LearnerTextSearchViewModel() { LearnerNumberLabel = LearnerNumberLabel, SearchText = searchText };
        PopulateNavigation(searchDownloadViewModel.TextSearchViewModel);

        var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
        var disabledTypes = await _downloadService.CheckForFENoDataAvailable(new string[] { selectedPupil }, downloadTypeArray, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
        SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);

        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.NonLearnerNumberDownloadOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.FurtherEducation.DownloadNonUlnFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadFurtherEducationFile(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.LearnerNumber))
        {
            var selectedPupils = model.LearnerNumber.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = await _downloadService.GetFECSVFile(selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NonUniqueLearnerNumber).ConfigureAwait(false);

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

        var selectedPupil = GetSelected();

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
    // @SONAR_IGNORE_START
    [NonAction]
    public async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerTextSearchViewModel model = new();

        PopulatePageText(model);
        PopulateNavigation(model);
        model.LearnerNumberLabel = LearnerNumberLabel;

        model.ShowMiddleNames = ShowMiddleNames;

        if (returnToSearch ?? false)
        {
            if (_sessionProvider.ContainsSessionKey(SearchSessionKey))
                model.SearchText = _sessionProvider.GetSessionValue(SearchSessionKey);

            if (_sessionProvider.ContainsSessionKey(SearchFiltersSessionKey))
                model.SearchFilters =
                    _sessionProvider.GetSessionValueOrDefault<SearchFilters>(SearchFiltersSessionKey);

            SetSortOptions(model);

            GetPersistedGenderFiltersForViewModel(model);
            GetPersistedSexFiltersForViewModel(model);
            model = await GenerateLearnerTextSearchViewModel(model, null, null, null, null, model.SortField, model.SortDirection);
            model.PageNumber = 0;
            model.PageSize = PAGESIZE;
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
       string surnameFilter, string middlenameFilter,
       string forenameFilter, string searchByRemove,
       int pageNumber,
       bool hasQueryItem = false,
       bool calledByController = false,
       string sortField = "",
       string sortDirection = "",
       bool resetSelection = false)
    {
        GetPersistedGenderFiltersForViewModel(model);
        GetPersistedSexFiltersForViewModel(model);
        model.SearchText = SecurityHelper.SanitizeText(model.SearchText);
        model.LearnerNumberLabel = LearnerNumberLabel;

        if (resetSelection || searchByRemove != null)
        {
            _selectionManager.Clear();
            ClearSortOptions();
            RemoveGenderFilterItemFromModel(model, searchByRemove);
            RemoveSexFilterItemFromModel(model, searchByRemove);

            SetPersistedGenderFiltersForViewModel(model);
            SetPersistedSexFiltersForViewModel(model);
        }

        if (resetSelection && searchByRemove == null)
        {
            RemoveAllGenderFilterItemsFromModel(model);
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

        model.ReturnRoute = ReturnRoute;

        PopulatePageText(model);
        PopulateNavigation(model);

        model.ShowMiddleNames = this.ShowMiddleNames;

        _sessionProvider.SetSessionValue(SearchSessionKey, model.SearchText);

        if (model.SearchFilters != null)
            _sessionProvider.SetSessionValue(SearchSessionKey, model.SearchFilters);

        return View(SearchView, model);
    }

    [NonAction]
    public virtual async Task<IActionResult> ReturnToSearch(LearnerTextSearchViewModel model)
    {
        if(_sessionProvider.ContainsSessionKey(SearchSessionKey))
            model.SearchText = _sessionProvider.GetSessionValue(SearchSessionKey);

        if (_sessionProvider.ContainsSessionKey(SearchFiltersSessionKey))
            model.SearchFilters =
                _sessionProvider.GetSessionValueOrDefault<SearchFilters>(SearchFiltersSessionKey);

        return await Search(model, null, null, null, null, model.PageNumber, calledByController: true, hasQueryItem: true, sortField: model.SortField, sortDirection: model.SortDirection);
    }


    [NonAction]
    public async Task<IActionResult> DobSearchFilter(LearnerTextSearchViewModel model)
    {
        var day = model.SearchFilters.CustomFilterText.DobDay;
        var month = model.SearchFilters.CustomFilterText.DobMonth;
        var year = model.SearchFilters.CustomFilterText.DobYear;

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
            var yearLimit = DateTime.Now.Year - 3;
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


    [NonAction]
    public async Task<IActionResult> SurnameSearchFilter(LearnerTextSearchViewModel model, string surnameFilter)
    {
        ModelState.Clear();

        if (!string.IsNullOrEmpty(surnameFilter))
        {
            model.SearchFilters.CustomFilterText.Surname = SecurityHelper.SanitizeText(surnameFilter);
        }

        if (String.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Surname))
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

        if (String.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Middlename))
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

        if (String.IsNullOrEmpty(model.SearchFilters.CustomFilterText.Forename))
        {
            ModelState.AddModelError("NoForenameFilter", Messages.Search.Errors.FilterEmpty);
            model.FilterErrors.ForenameError = true;
        }

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [NonAction]
    public async Task<IActionResult> GenderSearchFilter(LearnerTextSearchViewModel model)
    {
        SetPersistedGenderFiltersForViewModel(model);
        ModelState.Clear();
        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [NonAction]
    public async Task<IActionResult> SexSearchFilter(LearnerTextSearchViewModel model)
    {
        SetPersistedSexFiltersForViewModel(model);
        ModelState.Clear();
        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    private async Task<IActionResult> ReturnToRoute(LearnerTextSearchViewModel model)
    {
        _selectionManager.Clear();
        ClearSortOptions();

        return await Search(model, null, null, null, null, model.PageNumber, false);
    }

    private void GetPersistedGenderFiltersForViewModel(
        LearnerTextSearchViewModel model)
    {
        var genderFilters =
            TempData.GetPersistedObject<string[]>(
                PersistedSelectedGenderFiltersKey,
                keepTempDataBetweenRequests: true);

        if (genderFilters != null)
            model.SelectedGenderValues = genderFilters;
    }

    private void GetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model)
    {
        var sexFilters =
            TempData.GetPersistedObject<string[]>(
                PersistedSelectedSexFiltersKey,
                keepTempDataBetweenRequests: true);

        if (sexFilters != null)
            model.SelectedSexValues = sexFilters;
    }
    private void SetPersistedGenderFiltersForViewModel(
        LearnerTextSearchViewModel model) =>
            TempData.SetPersistedObject(
                model.SelectedGenderValues,
                PersistedSelectedGenderFiltersKey);

    private void SetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model) =>
        TempData.SetPersistedObject(
            model.SelectedSexValues,
            PersistedSelectedSexFiltersKey);

    private void RemoveGenderFilterItemFromModel(
        LearnerTextSearchViewModel model,
        string genderFilterItem)
    {
        if (!string.IsNullOrWhiteSpace(genderFilterItem))
        {
            model.SelectedGenderValues =
                model.SelectedGenderValues?.Where(selectedGenderValue =>
                    selectedGenderValue.SwitchGenderToParseName() != genderFilterItem).ToArray();
        }
    }

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

    private void RemoveAllGenderFilterItemsFromModel(
        LearnerTextSearchViewModel model)
    {
        model.SelectedGenderValues = null;
        SetPersistedGenderFiltersForViewModel(model);
        TempData.Remove(PersistedSelectedGenderFiltersKey);
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

        model.LearnerTextDatabaseName = LearnerTextDatabaseName;
        model.ShowMiddleNames = this.ShowMiddleNames;

        model = SetSearchFiltersUrls(model);

        if (ModelState.IsValid)
        {
            model.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
            model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
            model.MaximumResults = IndexType == AzureSearchIndexType.FurtherEducation ? _appSettings.MaximumNonULNResults : _appSettings.MaximumNonUPNResults;
            model.DownloadSelectedLink = DownloadSelectedLink;

            if (currentFilters.Count > 0)
            {
                model.SearchFilters.CurrentFiltersApplied = currentFilters;
            }

            model.SearchFilters.CurrentFiltersAppliedString = JsonSerializer.Serialize(currentFilters);

        }

        IList<FilterRequest> filterRequests =
            _filtersRequestMapper.Map(
                _filtersRequestBuilder
                    .GenerateFilterRequest(model, currentFilters));

        SortOrder sortOrder =
            _sortOrderViewModelToRequestMapper.Map((sortField, sortDirection));

        SearchByKeyWordsResponse searchResponse =
            await _furtherEducationSearchUseCase.HandleRequestAsync(
                new SearchByKeyWordsRequest(
                    searchKeywords: model.SearchText,
                    filterRequests: filterRequests,
                    sortOrder: sortOrder,
                    offset: model.Offset))
            .ConfigureAwait(false);

        return _learnerSearchResponseToViewModelMapper.Map(
            LearnerSearchMappingContext.Create(model, searchResponse));
    }

    private List<CurrentFilterDetail> SetCurrentFilters(LearnerTextSearchViewModel model,
       string surnameFilter, string middlenameFilter, string forenameFilter, string searchByRemove)
    {
        List<CurrentFilterDetail> currentFilters =
            !string.IsNullOrEmpty(model.SearchFilters.CurrentFiltersAppliedString)
                ? JsonSerializer.Deserialize<List<CurrentFilterDetail>>(model.SearchFilters.CurrentFiltersAppliedString) : [];

        currentFilters = CheckDobFilter(model, currentFilters);
        currentFilters = CheckGenderFilter(model, currentFilters);
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

    private List<CurrentFilterDetail> CheckTextFilters(LearnerTextSearchViewModel model, List<CurrentFilterDetail> currentFilters,
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

    private void AddNameFilter(ref List<CurrentFilterDetail> currentFilters, FilterType filterType, string filterValue)
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

    private List<CurrentFilterDetail> RemoveFilterValue(string searchByRemove, List<CurrentFilterDetail> currentFilters, LearnerTextSearchViewModel model)
    {
        if (!string.IsNullOrEmpty(searchByRemove))
        {
            var item = currentFilters.Find(x => x.FilterName == searchByRemove);
            if (item != null)
            {
                currentFilters.Remove(item);
            }
            var genderFiltersActive = currentFilters.Find(x => x.FilterType == FilterType.Gender);
            if (genderFiltersActive != null && model.SelectedGenderValues == null && currentFilters.Count() >= 1)
            {
                List<string> currentSelectedGenderList = new List<string>();
                foreach (var filter in currentFilters)
                {
                    currentSelectedGenderList.Add(filter.FilterName.Substring(0, 1));
                }
                model.SelectedGenderValues = currentSelectedGenderList.ToArray();
            }
            var sexFiltersActive = currentFilters.Find(x => x.FilterType == FilterType.Sex);
            if (sexFiltersActive != null && model.SelectedSexValues == null && currentFilters.Count() >= 1)
            {
                List<string> currentSelectedSexList = new List<string>();
                foreach (var filter in currentFilters)
                {
                    currentSelectedSexList.Add(filter.FilterName.Substring(0, 1));
                }
                model.SelectedSexValues = currentSelectedSexList.ToArray();
            }
        }
        return currentFilters;
    }

    private LearnerTextSearchViewModel SetSearchFiltersUrls(LearnerTextSearchViewModel model)
    {
        model.RedirectUrls.SurnameFilterURL = SurnameFilterUrl;
        model.RedirectUrls.FormAction = FormAction;
        model.RedirectUrls.RemoveAction = RemoveActionUrl;
        model.RedirectUrls.DobFilterUrl = DobFilterUrl;
        model.RedirectUrls.ForenameFilterUrl = ForenameFilterUrl;
        model.RedirectUrls.MiddlenameFilterUrl = MiddlenameFilterUrl;
        model.RedirectUrls.GenderFilterUrl = GenderFilterUrl;
        model.RedirectUrls.SexFilterUrl = SexFilterUrl;

        return model;
    }

    private List<CurrentFilterDetail> CheckGenderFilter(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters)
    {
        if (model.SelectedGenderValues?.Length > 0)
        {
            RemoveAllGenderFilters(currentFilters);
            AddSelectedGenderValuesToCurrentFilters(model, currentFilters);
        }
        else if (model.SelectedGenderValues == null && currentFilters.Count > 0)
        {
            RemoveSelectedFilterValueFromCurrentFilters(currentFilters, model);
            model.SelectedGenderValues = null;
        }
        return currentFilters;
    }

    private List<CurrentFilterDetail> CheckSexFilter(
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

    private void AddSelectedGenderValuesToCurrentFilters(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters)
    {
        model.SelectedGenderValues.Distinct().ToList().ForEach(genderFilterItem =>
        {
            currentFilters.Add(
                new CurrentFilterDetail
                {
                    FilterType = FilterType.Gender,
                    FilterName = genderFilterItem.SwitchGenderToParseName()
                });
        });
    }

    private void AddSelectedSexValuesToCurrentFilters(
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
    private void RemoveAllGenderFilters(
        List<CurrentFilterDetail> currentFilters) =>
            currentFilters.RemoveAll(currentFilterDetail =>
                currentFilterDetail.FilterType == FilterType.Gender);

    private void RemoveAllSexFilters(
        List<CurrentFilterDetail> currentFilters) =>
        currentFilters.RemoveAll(currentFilterDetail =>
            currentFilterDetail.FilterType == FilterType.Sex);

    private IEnumerable<string> ExtractGenderValuesFromCurrentFilterDetail(
        IEnumerable<CurrentFilterDetail> currentFilters) =>
            currentFilters.Where(currentFilterDetail =>
                    currentFilterDetail.FilterType == FilterType.Gender)
                        .Select(currentFilterDetail =>
                            currentFilterDetail.FilterName);

    private IEnumerable<string> ExtractSexValuesFromCurrentFilterDetail(
        IEnumerable<CurrentFilterDetail> currentFilters) =>
        currentFilters.Where(currentFilterDetail =>
                currentFilterDetail.FilterType == FilterType.Sex)
            .Select(currentFilterDetail =>
                currentFilterDetail.FilterName);

    private void RemoveSelectedFilterValueFromCurrentFilters(
        List<CurrentFilterDetail> currentFilters,
        LearnerTextSearchViewModel model)
    {
        IEnumerable<string> currentFiltersGender =
                ExtractGenderValuesFromCurrentFilterDetail(currentFilters);

        currentFiltersGender.ToList().ForEach(gender =>
                currentFilters = RemoveFilterValue(gender, currentFilters, model));

        IEnumerable<string> currentFiltersSex =
            ExtractSexValuesFromCurrentFilterDetail(currentFilters);

        currentFiltersSex.ToList().ForEach(sex =>
            currentFilters = RemoveFilterValue(sex, currentFilters, model));
    }

    private List<CurrentFilterDetail> CheckDobFilter(LearnerTextSearchViewModel model, List<CurrentFilterDetail> currentFilters)
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
        if(_sessionProvider.ContainsSessionKey(SortDirectionKey))
            model.SortDirection = _sessionProvider.GetSessionValue(SortDirectionKey);
        
        if (_sessionProvider.ContainsSessionKey(SortFieldKey))
            model.SortField = _sessionProvider.GetSessionValue(SortFieldKey);
    }

    private void ClearSortOptions()
    {
        _sessionProvider.RemoveSessionValue(SortDirectionKey);
        _sessionProvider.RemoveSessionValue(SortFieldKey);
    }

    protected LearnerTextSearchViewModel PopulatePageText(LearnerTextSearchViewModel model)
    {
        model.PageHeading = PageHeading;
        model.ShowGender = ShowGender;
        model.ShowLocalAuthority = ShowLocalAuthority;
        return model;
    }

    protected LearnerTextSearchViewModel PopulateNavigation(LearnerTextSearchViewModel model)
    {
        model.LearnerTextSearchController = SearchController;
        model.LearnerTextSearchAction = SearchAction;

        model.LearnerNumberController = SearchLearnerNumberController;
        model.LearnerNumberAction = SearchLearnerNumberAction;
        model.RedirectUrls.FormAction = RedirectUrlFormAction;

        model.LearnerTextDatabaseName = LearnerTextDatabaseName;
        model.RedirectUrls.DobFilterUrl = DobFilterUrl;
        model.RedirectFrom = RedirectFrom;
        model.DownloadLinksPartial = DownloadLinksPartial;

        model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;

        return model;
    }

    protected virtual void SetSelections(string selected)
    {
        if (!string.IsNullOrEmpty(selected))
        {
            _selectionManager.Clear();
            _selectionManager.Add(selected);
        }
    }

    protected string GetSelected()
    {
        return _selectionManager.GetSelectedFromSession();
    }

    // @SONAR_IGNORE_END
    #endregion
}
