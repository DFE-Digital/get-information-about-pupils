using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Controllers;

/// <summary>
/// Provides a base implementation for search controllers that use learner numbers (UPN/ULN)
/// This is unit tested via its concrete implementations
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class BaseLearnerNumberController : Controller
{
    public const int PAGESIZE = 20;
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";
    public const string TOTAL_SEARCH_RESULTS = "totalSearch";

    private readonly ILogger<BaseLearnerNumberController> _logger;
    private readonly IPaginatedSearchService _paginatedSearch;
    protected readonly ISelectionManager _selectionManager;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    private readonly AzureAppSettings _appSettings;
    private readonly IJsonSerializer _jsonSerializer;

    public abstract string PageHeading { get; }

    public abstract string SearchAction { get; }
    public abstract string FullTextLearnerSearchController { get; }
    public abstract string FullTextLearnerSearchAction { get; }
    public abstract string InvalidUPNsConfirmationAction { get; }
    public abstract string DownloadLinksPartial { get; }
    public abstract AzureSearchIndexType IndexType { get; }
    public abstract string SearchSessionKey { get; }
    public abstract string SearchSessionSortField { get; }
    public abstract string SearchSessionSortDirection { get; }
    public abstract string DownloadSelectedLink { get; }

    public BaseLearnerNumberController(ILogger<BaseLearnerNumberController> logger,
        IPaginatedSearchService paginatedSearch,
        ISelectionManager selectionManager,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IJsonSerializer jsonSerializer)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(paginatedSearch);
        _paginatedSearch = paginatedSearch;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;
    }

    [NonAction]
    public async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerNumberSearchViewModel model = new();

        PopulatePageText(model);
        PopulateNavigation(model);
        PopulateSorting(
            model,
            HttpContext.Session.GetString(SearchSessionSortField),
            HttpContext.Session.GetString(SearchSessionSortDirection));

        ClearSortingDataFromSession();
        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;

        SetModelApplicationLabels(model);

        if (returnToSearch ?? false && HttpContext.Session.Keys.Contains(SearchSessionKey))
        {
            ModelState.Clear();
            model.LearnerNumber = HttpContext.Session.GetString(SearchSessionKey);
            model = await GetPupilsForSearchBuilder(model, IndexType, 0, true).ConfigureAwait(false);
            model.PageNumber = 0;
            model.PageSize = PAGESIZE;
        }

        if (!returnToSearch.HasValue)
        {
            _selectionManager.Clear();
        }

        return View(Global.SearchView, model);
    }

    [NonAction]
    public async Task<IActionResult> Search(LearnerNumberSearchViewModel model, int pageNumber, string sortField = "", string sortDirection = "", bool hasQueryItem = false, bool calledByController = false, bool resetSelections = false)
    {
        _logger.LogInformation("BaseLearnerNumberController POST method called");
        if (resetSelections)
        {
            _selectionManager.Clear();
            ClearSortingDataFromSession();
        }

        PopulatePageText(model);
        PopulateNavigation(model);
        PopulateSorting(model, sortField, sortDirection);

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        if (!string.IsNullOrEmpty(model.LearnerNumber))
        {
            model.LearnerNumber = Regex.Replace(model.LearnerNumber, @"[ \t]", "");
        }

        bool notPaged = hasQueryItem && !calledByController;
        bool allSelected = false;

        model.SearchBoxErrorMessage =
            ModelState.IsValid is false ?
                PupilHelper.GenerateValidationMessageUpnSearch(ModelState) :
                    null;

        model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);

        if (ModelState.IsValid)
        {
            SetModelApplicationLabels(model);

            if (!string.IsNullOrEmpty(model.SelectAllNoJsChecked))
            {
                bool selectAll = Convert.ToBoolean(model.SelectAllNoJsChecked);
                string[] LearnerNumbers = model.LearnerNumberIds.FormatLearnerNumbers();
                if (selectAll)
                {
                    _selectionManager.AddAll(LearnerNumbers);
                    model.ToggleSelectAll = true;
                }
                else
                {
                    _selectionManager.RemoveAll(LearnerNumbers);
                    model.ToggleSelectAll = false;
                }

                model.SelectAllNoJsChecked = null;
                allSelected = true;
            }

            if (!notPaged && !allSelected)
            {
                SetSelections(
                    model.PageLearnerNumbers.Split(','),
                    model.SelectedPupil);
            }

            model.PageNumber = pageNumber;
            model.PageSize = PAGESIZE;

            model = await GetPupilsForSearchBuilder(
                model,
                IndexType,
                pageNumber,
                notPaged).ConfigureAwait(false);
        }

        HttpContext.Session.SetString(SearchSessionKey, model.LearnerNumber);

        _logger.LogInformation("BaseLearnerNumberController POST search method invoked");

        return View(Global.SearchView, model);
    }

    private void SetModelApplicationLabels(LearnerNumberSearchViewModel model)
    {
        model.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
        model.DownloadSelectedLink = DownloadSelectedLink;
        model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
    }


    [NonAction]
    public async Task<IActionResult> InvalidUPNs(InvalidLearnerNumberSearchViewModel model)
    {
        _logger.LogInformation("National pupil database Upn Invalid UPNs POST method called");

        model.SearchAction = SearchAction;
        model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;

        model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);

        model = await GetInvalidPupils(model, IndexType).ConfigureAwait(false);

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
    public async Task<IActionResult> AddToMyPupilList(LearnerNumberSearchViewModel model)
    {
        PopulatePageText(model);
        PopulateNavigation(model);

        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        HashSet<string> selected = GetSelected(model.LearnerNumberIds.FormatLearnerNumbers());

        if (selected.Count == 0)
        {
            model.NoPupil = true;
            model.NoPupilSelected = true;
            return await ReturnToPage(model);
        }

        List<string> invalidUPNs = selected.Where(x => !ValidationHelper.IsValidUpn(x)).ToList();

        if (invalidUPNs.Count > 0)
        {
            return await InvalidUPNs(new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = string.Join("\n", invalidUPNs)
            });
        }

        model.SelectedPupil = model.SelectedPupil.Except(invalidUPNs).ToList();
        selected = selected.Where(t => !invalidUPNs.Contains(t)).ToHashSet();

        try
        {
            await _addPupilsToMyPupilsUseCase.HandleRequestAsync(
                new AddPupilsToMyPupilsRequest(
                    userId: User.GetUserId(),
                    pupils: selected));
        }

        catch (MyPupilsLimitExceededException)
        {
            model.ErrorDetails = Messages.Common.Errors.MyPupilListLimitExceeded;
            return await ReturnToPage(model);
        }

        model.ItemAddedToMyPupilList = true;
        return await ReturnToPage(model);
    }

    private async Task<LearnerNumberSearchViewModel> GetPupilsForSearchBuilder(
        LearnerNumberSearchViewModel model,
        AzureSearchIndexType indexType,
        int pageNumber,
        bool first)
    {
        if (string.IsNullOrEmpty(model.LearnerNumber))
        {
            return model;
        }

        string[] learnerNumberArray = model.LearnerNumber.FormatLearnerNumbers();

        string searchText = model.LearnerNumber.ToSearchText();

        if (!first)
        {
            searchText = model.LearnerIdSearchResult;
        }

        PaginatedResponse result = await _paginatedSearch.GetPage(
            searchText,
            null,
            first ? _appSettings.MaximumUPNsPerSearch : PAGESIZE,
            pageNumber,
            indexType,
            first ? AzureSearchQueryType.Numbers : AzureSearchQueryType.Id,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            model.SortField,
            model.SortDirection
            );

        model.Total = result.Count ?? result.Learners.Count;

        List<string> idList = SetLearnerNumberIds(result);

        string[] combinedIdLearnerNumberArray = learnerNumberArray.Concat(idList).ToArray();

        if (first)
        {
            model.LearnerIdSearchResult = result.GetLearnerIdsAsString();

            HashSet<string> learnerNumberIdSet = result.GetLearnerNumberIds();
            HashSet<string> learnerNumberSet = result.GetLearnerNumbers();
            model.LearnerNumberIds = string.Join("\n", learnerNumberIdSet);

            IEnumerable<string> missing = combinedIdLearnerNumberArray.Except(learnerNumberIdSet).Except(learnerNumberSet);

            HttpContext.Session.SetString(
                MISSING_LEARNER_NUMBERS_KEY, _jsonSerializer.Serialize(missing));

            HttpContext.Session.SetString(
                TOTAL_SEARCH_RESULTS, model.Total.ToString());
        }
        else
        {
            model.Total = Convert.ToInt32(HttpContext.Session.GetString(TOTAL_SEARCH_RESULTS));
        }
#nullable enable
        _jsonSerializer.TryDeserialize(HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY), out List<string>? notFound);
        model.NotFound = notFound;
#nullable restore

        List<string> duplicateLearnerNumbers = ValidationHelper.GetDuplicates(learnerNumberArray.ToList());

        if (duplicateLearnerNumbers.Any())
        {
            foreach (string learnerNumber in duplicateLearnerNumbers)
            {
                model.Duplicates.Add(learnerNumber);
            }
        }

        List<string> potentialErrorLearnerNumbers = learnerNumberArray.Distinct().ToList();

        if (potentialErrorLearnerNumbers.Any())
        {
            foreach (string learnerNumber in potentialErrorLearnerNumbers)
            {
                bool isValid = ValidationHelper.IsValidUpn(learnerNumber);

                if (!isValid)
                {
                    model.Invalid.Add(learnerNumber);
                }
            }

            _logger.LogError("Some of the LearnerNumber(s) have not been found in our database");
            //result.Reverse(); // TODO: why is this here?
        }

        // ensure that the selections are set appropriately
        HashSet<string> selected = GetSelected(combinedIdLearnerNumberArray);
        foreach (Learner learner in result.Learners)
        {
            learner.Selected = selected.Contains(learner.LearnerNumberId);
        }

        model.Learners = first ? result.Learners.Take(PAGESIZE) : result.Learners;
        model.PageLearnerNumbers = string.Join(',', model.Learners.Select(l => l.LearnerNumberId));
        return model;
    }

    private async Task<InvalidLearnerNumberSearchViewModel> GetInvalidPupils(InvalidLearnerNumberSearchViewModel model, AzureSearchIndexType indexType)
    {
        if (string.IsNullOrEmpty(model.LearnerNumber))
        {
            return model;
        }

        string searchInput = model.LearnerNumber.ToDecryptedSearchText();

        PaginatedResponse result = await _paginatedSearch.GetPage(
          searchInput,
            null,
             _appSettings.MaximumUPNsPerSearch,
            0,
            indexType,
            AzureSearchQueryType.Numbers,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
            );

        model.Learners = result.Learners ?? [];

        PaginatedResponse nonUPNResult = await _paginatedSearch.GetPage(
            searchInput,
            null,
             _appSettings.MaximumUPNsPerSearch,
            0,
            indexType,
            AzureSearchQueryType.Id,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
        );

        model.Learners = model.Learners.Union(nonUPNResult.Learners);

        return model;
    }

    private List<string> SetLearnerNumberIds(PaginatedResponse result)
    {
        List<string> idList = [];
        foreach (Learner learner in result.Learners)
        {
            switch (learner.LearnerNumber)
            {
                case "0":
                    learner.LearnerNumberId = learner.Id;
                    idList.Add(learner.Id);
                    break;

                default:
                    learner.LearnerNumberId = learner.LearnerNumber;
                    break;
            }
            ;
        }

        return idList;
    }

    protected HashSet<string> GetSelected(string[] available)
    {
        // ensure we remove the missing items
        List<string> missing = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));

        if (missing != null)
        {
            string[] actuallyAvailable = available.Except(missing).ToArray();
            return _selectionManager.GetSelected(actuallyAvailable);
        }

        return _selectionManager.GetSelected(available);
    }

    protected virtual void SetSelections(IEnumerable<string> available, IEnumerable<string> selected)
    {
        IEnumerable<string> toAdd;
        IEnumerable<string> toRemove;

        if (selected != null)
        {
            toAdd = selected;
            toRemove = available.Except(selected);
        }
        else
        {
            toAdd = new List<string>();
            toRemove = available; // nothing selected, remove them all.
        }

        _selectionManager.AddAll(toAdd);
        _selectionManager.RemoveAll(toRemove);
    }

    protected abstract Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model);

    protected LearnerNumberSearchViewModel PopulatePageText(LearnerNumberSearchViewModel model)
    {
        model.PageHeading = PageHeading;
        model.LearnerNumberLabel = Global.LearnerNumberLabel;
        model.ShowLocalAuthority = true;
        model.ShowMiddleNames = true;
        return model;
    }

    protected LearnerNumberSearchViewModel PopulateNavigation(LearnerNumberSearchViewModel model)
    {
        model.DownloadLinksPartial = DownloadLinksPartial;
        model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;
        model.SearchAction = SearchAction;
        model.FullTextLearnerSearchController = FullTextLearnerSearchController;
        model.FullTextLearnerSearchAction = FullTextLearnerSearchAction;
        return model;
    }

    private LearnerNumberSearchViewModel PopulateSorting(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
    {
        if (!string.IsNullOrEmpty(sortField) && !string.IsNullOrEmpty(sortDirection))
        {
            SetSortingDataIntoSession(sortField, sortDirection);
            SetSortingDataIntoModel(model, sortField, sortDirection);
        }
        else if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SearchSessionSortField))
                && !string.IsNullOrEmpty(HttpContext.Session.GetString(SearchSessionSortDirection)))
        {
            SetSortingDataIntoModel(
                model,
                HttpContext.Session.GetString(SearchSessionSortField),
                HttpContext.Session.GetString(SearchSessionSortDirection));
        }

        return model;
    }

    // Stores sorting data into session to make it reachable on returnToSearch
    protected void SetSortingDataIntoSession(string sortField, string sortDirection)
    {
        HttpContext.Session.SetString(SearchSessionSortField, sortField);
        HttpContext.Session.SetString(SearchSessionSortDirection, sortDirection);
    }

    protected LearnerNumberSearchViewModel SetSortingDataIntoModel(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
    {
        model.SortField = sortField;
        model.SortDirection = sortDirection;
        return model;
    }

    private void ClearSortingDataFromSession()
    {
        if (HttpContext.Session.Keys.Contains(SearchSessionSortField) &&
                HttpContext.Session.Keys.Contains(SearchSessionSortDirection))
        {
            HttpContext.Session.Remove(SearchSessionSortField);
            HttpContext.Session.Remove(SearchSessionSortDirection);
        }
    }
}
