using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Controllers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch;

[ExcludeFromCodeCoverage]
public abstract class BaseLearnerTextSearchController : Controller
{
    public const int PAGESIZE = 20;
    private const string PersistedSelectedSexFiltersKey = "PersistedSelectedSexFilters";
    private readonly ILogger<BaseLearnerTextSearchController> _logger;
    private readonly IPaginatedSearchService _paginatedSearch;
    private readonly ITextSearchSelectionManager _selectionManager;
    private readonly ISessionProvider _sessionProvider;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    protected readonly AzureAppSettings _appSettings;

    public abstract string PageHeading { get; }
    public abstract string SearchSessionKey { get; }
    public abstract string SearchFiltersSessionKey { get; }
    public abstract string SortDirectionKey { get; }
    public abstract string SortFieldKey { get; }
    public abstract string SearchAction { get; }
    public abstract string SearchController { get; }
    public string SearchLearnerNumberController => Routes.Application.Search;
    public abstract string SearchLearnerNumberAction { get; }
    public abstract string RedirectUrlFormAction { get; }
    public abstract string RedirectFrom { get; }
    public abstract string LearnerTextDatabaseName { get; }
    public abstract string FormAction { get; }
    public abstract string RemoveActionUrl { get; }
    public string SearchView => Global.NonUpnSearchView;
    public abstract string DobFilterUrl { get; }
    public abstract string ForenameFilterUrl { get; }
    public abstract string MiddlenameFilterUrl { get; }
    public abstract string SurnameFilterUrl { get; }
    public abstract string SexFilterUrl { get; }
    public abstract string DownloadLinksPartial { get; }
    public abstract AzureSearchIndexType IndexType { get; }
    public abstract ReturnRoute ReturnRoute { get; }
    public abstract string InvalidUPNsConfirmationAction { get; }
    public string LearnerNumberLabel => Global.LearnerNumberLabel;
    public abstract string DownloadSelectedLink { get; }


    public BaseLearnerTextSearchController(ILogger<BaseLearnerTextSearchController> logger,
        IPaginatedSearchService paginatedSearch,
        ITextSearchSelectionManager selectionManager,
        IOptions<AzureAppSettings> azureAppSettings,
        ISessionProvider sessionProvider,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(paginatedSearch);
        _paginatedSearch = paginatedSearch;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;
        
        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;
    }


    [NonAction]
    public async Task<IActionResult> Search(bool? returnToSearch)
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

        if (!String.IsNullOrEmpty(sortField) || !String.IsNullOrEmpty(sortDirection))
        {
            this.HttpContext.Session.SetString(SortFieldKey, sortField);
            this.HttpContext.Session.SetString(SortDirectionKey, sortDirection);
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

    [NonAction]
    public virtual async Task<IActionResult> ReturnToSearch(LearnerTextSearchViewModel model)
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


    [NonAction]
    public async Task<IActionResult> InvalidUPNs(InvalidLearnerNumberSearchViewModel model)
    {
        _logger.LogInformation("National pupil database Upn Invalid UPNs POST method called");

        model.SearchAction = SearchAction;
        model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;

        model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);

        model = await GetInvalidPupil(model, IndexType).ConfigureAwait(false);

        _logger.LogInformation("National pupil database Upn Invalid UPNs POST search method invoked");

        return View(Global.InvalidUPNsView, model);
    }

    [NonAction]
    public async Task<IActionResult> InvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
    {
        model.SearchAction = SearchAction;
        model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;

        if (!string.IsNullOrEmpty(model.SelectedInvalidUPNOption))
        {
            switch (model.SelectedInvalidUPNOption)
            {
                case Global.InvalidUPNConfirmation_ReturnToSearch:

                    return RedirectToAction(model.SearchAction, new { returnToSearch = true });

                case Global.InvalidUPNConfirmation_MyPupilList: return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
            }
        }
        else
        {
            ModelState.AddModelError("NoContinueSelection", Messages.Common.Errors.NoContinueSelection);
        }

        return await InvalidUPNs(model);
    }

    [NonAction]
    public async Task<IActionResult> AddToMyPupilList(LearnerTextSearchViewModel model)
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
        PaginatedResponse result = await _paginatedSearch.GetPage(
            model.SearchText,
            requestFilters,
            first ? _appSettings.MaximumNonUPNResults : PAGESIZE,
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

        model.PageLearnerNumbers = String.Join(',', model.Learners.Select(l => l.LearnerNumberId));

        model.ShowOverLimitMessage = model.Total > 100000;

        return model;
    }

    private async Task<InvalidLearnerNumberSearchViewModel> GetInvalidPupil(InvalidLearnerNumberSearchViewModel model, AzureSearchIndexType indexType)
    {
        string searchInput = model.LearnerNumber.ToDecryptedSearchText();

        PaginatedResponse result = await _paginatedSearch.GetPage(
          searchInput,
            null,
            1,
            0,
            indexType,
            AzureSearchQueryType.Numbers,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
            );

        model.Learners = result.Learners ?? new List<Learner>();

        PaginatedResponse nonUPNResult = await _paginatedSearch.GetPage(
        searchInput,
        null,
        1,
        0,
        indexType,
        AzureSearchQueryType.Id,
        AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
        );

        model.Learners = model.Learners.Union(nonUPNResult.Learners);
        model.Learners.ToList().ForEach(x => x.LearnerNumberId = x.LearnerNumber);
        int lowAge = User.GetOrganisationLowAge();
        int highAge = User.GetOrganisationHighAge();

        bool isAdmin = User.IsAdmin();
        return model;
    }

    private Dictionary<string, string[]> GenerateRequestModel(LearnerTextSearchViewModel model, List<CurrentFilterDetail> currentFilters)
    {
        Dictionary<string, string[]> requestFilters = new Dictionary<string, string[]>();
        List<string> surnameList = new List<string>();
        List<string> middlenameList = new List<string>();
        List<string> forenameList = new List<string>();
        List<string> dobList = new List<string>();

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

    private void ParseSex(ref PaginatedResponse result)
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

    protected List<CurrentFilterDetail> SetCurrentFilters(LearnerTextSearchViewModel model,
       string surnameFilter, string middlenameFilter, string forenameFilter, string searchByRemove)
    {
        List<CurrentFilterDetail> currentFilters = !string.IsNullOrEmpty(model.SearchFilters.CurrentFiltersAppliedString)
                                                   ? JsonSerializer.Deserialize<List<CurrentFilterDetail>>(model.SearchFilters.CurrentFiltersAppliedString)
                                                   : new List<CurrentFilterDetail>();

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

    protected LearnerTextSearchViewModel SetSearchFiltersUrls(LearnerTextSearchViewModel model)
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

    protected void SetSortOptions(LearnerTextSearchViewModel model)
    {
        if (this.HttpContext.Session.Keys.Contains(SortDirectionKey))
            model.SortDirection = this.HttpContext.Session.GetString(SortDirectionKey);
        if (this.HttpContext.Session.Keys.Contains(SortFieldKey))
            model.SortField = this.HttpContext.Session.GetString(SortFieldKey);
    }

    protected void ClearSortOptions()
    {
        HttpContext.Session.Remove(SortDirectionKey);
        HttpContext.Session.Remove(SortFieldKey);
    }


    private void SetLearnerNumberId(LearnerTextSearchViewModel model)
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

    protected LearnerTextSearchViewModel PopulatePageText(LearnerTextSearchViewModel model)
    {
        model.PageHeading = PageHeading;
        model.ShowMiddleNames = true;
        model.ShowLocalAuthority = true;
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
}
