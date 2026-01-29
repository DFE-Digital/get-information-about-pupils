using System.Text.RegularExpressions;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;

[Route(Routes.Application.Search)]
public class PupilPremiumLearnerNumberController : Controller
{
    private const int PAGESIZE = 20;
    private const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";
    private const string TOTAL_SEARCH_RESULTS = "totalSearch";

    private readonly ILogger<PupilPremiumLearnerNumberController> _logger;

    private readonly IUseCase<
        PupilPremiumSearchRequest,
        PupilPremiumSearchResponse> _searchUseCase;

    private readonly IMapper<
        PupilPremiumLearnerNumericSearchMappingContext,
        LearnerNumberSearchViewModel> _learnerNumericSearchResponseToViewModelMapper;

    private readonly ISelectionManager _selectionManager;
    private readonly IOptions<AzureAppSettings> _azureAppSettings;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;
    private readonly IMapper<SortOrderRequest, SortOrder> _sortOrderViewModelToRequestMapper;

    public string SearchAction => nameof(PupilPremium);
    public string FullTextLearnerSearchController => Global.PPNonUpnController;
    public string FullTextLearnerSearchAction => "NonUpnPupilPremiumDatabase";
    public string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchPupilPremiumDownloadLinks.cshtml";
    public AzureSearchIndexType IndexType => AzureSearchIndexType.PupilPremium;
    public string SearchSessionKey => "SearchPPUPN_SearchText";
    public string SearchSessionSortField => "SearchPPUPN_SearchTextSortField";
    public string SearchSessionSortDirection => "SearchPPUPN_SearchTextSortDirection";

    public PupilPremiumLearnerNumberController(
        ILogger<PupilPremiumLearnerNumberController> logger,
        IUseCase<
            PupilPremiumSearchRequest,
            PupilPremiumSearchResponse> searchUseCase,
        IMapper<SortOrderRequest, SortOrder> sortOrderViewModelToRequestMapper,
        IMapper<
            PupilPremiumLearnerNumericSearchMappingContext,
            LearnerNumberSearchViewModel> learnerNumericSearchResponseToViewModelMapper,
        ISelectionManager selectionManager,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IJsonSerializer jsonSerializer,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(searchUseCase);
        _searchUseCase = searchUseCase;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _azureAppSettings = azureAppSettings;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;

        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupilsService);
        _downloadPupilPremiumDataForPupilsService = downloadPupilPremiumDataForPupilsService;

        ArgumentNullException.ThrowIfNull(sortOrderViewModelToRequestMapper);
        _sortOrderViewModelToRequestMapper = sortOrderViewModelToRequestMapper;

        ArgumentNullException.ThrowIfNull(learnerNumericSearchResponseToViewModelMapper);
        _learnerNumericSearchResponseToViewModelMapper = learnerNumericSearchResponseToViewModelMapper;
    }


    [Route(Routes.Application.PupilPremium)]
    [HttpGet]
    public async Task<IActionResult> PupilPremium(bool? returnToSearch)
    {
        _logger.LogInformation("Pupil Premium Upn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.Application.PupilPremium)]
    [HttpPost]
    public async Task<IActionResult> PupilPremium(
        [FromForm] LearnerNumberSearchViewModel model,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("Pupil Premium Upn POST method called");
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
    [Route("add-pp-to-my-pupil-list")]
    public async Task<IActionResult> PPAddToMyPupilList(LearnerNumberSearchViewModel model)
    {
        return await AddToMyPupilList(model);
    }


    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(LearnerNumberSearchViewModel searchViewModel, CancellationToken ctx = default)
    {
        SetSelections(
            searchViewModel.PageLearnerNumbers.Split(','),
            searchViewModel.SelectedPupil);

        HashSet<string> selectedPupils = GetSelected(searchViewModel.LearnerNumberIds.FormatLearnerNumbers());

        if (selectedPupils.Count == 0)
        {
            searchViewModel.NoPupil = true;
            searchViewModel.NoPupilSelected = true;

            return await PupilPremium(
                searchViewModel,
                searchViewModel.PageNumber,
                HttpContext.Session.GetString(SearchSessionSortField),
                HttpContext.Session.GetString(SearchSessionSortDirection),
                true);
        }

        DownloadPupilPremiumFilesResponse result =
            await _downloadPupilPremiumDataForPupilsService.DownloadAsync(
                pupilUpns: selectedPupils,
                downloadEventType: Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                ctx);

        if (result.HasData)
        {
            return result.GetResult();
        }

        searchViewModel.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;

        return await PupilPremium(
            searchViewModel,
            searchViewModel.PageNumber,
            HttpContext.Session.GetString(SearchSessionSortField),
            HttpContext.Session.GetString(SearchSessionSortDirection),
            true);
    }


    protected async Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model)
    {
        return await PupilPremium(model, model.PageNumber, model.SortField, model.SortDirection, true);
    }

    #region WIP OLD INHERITED METHODS

    private async Task<IActionResult> AddToMyPupilList(LearnerNumberSearchViewModel model)
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
            model.Invalid = invalidUPNs;
            model.ErrorDetails = Messages.Common.Errors.InvalidPupilIdentifier;
            model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);
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

    private async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerNumberSearchViewModel model = new();

        PopulatePageText(model);
        PopulateNavigation(model);
        PopulateSorting(
            model,
            HttpContext.Session.GetString(SearchSessionSortField),
            HttpContext.Session.GetString(SearchSessionSortDirection));

        ClearSortingDataFromSession();
        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _azureAppSettings.Value.MaximumUPNsPerSearch;

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

    private async Task<IActionResult> Search(LearnerNumberSearchViewModel model, int pageNumber, string sortField = "", string sortDirection = "", bool hasQueryItem = false, bool calledByController = false, bool resetSelections = false)
    {
        _logger.LogInformation("PupilPremium Search method called");
        if (resetSelections)
        {
            _selectionManager.Clear();
            ClearSortingDataFromSession();
        }

        PopulatePageText(model);
        PopulateNavigation(model);
        PopulateSorting(model, sortField, sortDirection);

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _azureAppSettings.Value.MaximumUPNsPerSearch;
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

        _logger.LogInformation("PupilPremium Search method finished");

        return View(Global.SearchView, model);
    }

    private HashSet<string> GetSelected(string[] available)
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

    private void SetSelections(IEnumerable<string> available, IEnumerable<string> selected)
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

        SortOrder sortOrder = _sortOrderViewModelToRequestMapper.Map(
            new SortOrderRequest(
                searchKey: "pupil-premium",
                sortOrder: (model.SortField, model.SortDirection)));

        IList<FilterRequest> filterRequests =
        [
            new FilterRequest(
                filterName: "UPN",
                filterValues: learnerNumberArray)
        ];

        PupilPremiumSearchResponse searchResponse = await _searchUseCase.HandleRequestAsync(
            new PupilPremiumSearchRequest(
                searchKeywords: string.Join(" AND ", learnerNumberArray),
                filterRequests: filterRequests,
                sortOrder: sortOrder,
                model.Offset));

        LearnerNumberSearchViewModel result =
            _learnerNumericSearchResponseToViewModelMapper.Map(
                PupilPremiumLearnerNumericSearchMappingContext.Create(model, searchResponse));

        List<string> idList = SetLearnerNumberIds(result.Learners);

        string[] combinedIdLearnerNumberArray = learnerNumberArray.Concat(idList).ToArray();

        if (first)
        {
            model.LearnerIdSearchResult = GetLearnerIdsAsString(result.Learners);

            HashSet<string> learnerNumberIdSet = GetLearnerNumberIds(result.Learners);
            HashSet<string> learnerNumberSet = GetLearnerNumberIds(result.Learners);
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

    private LearnerNumberSearchViewModel PopulatePageText(LearnerNumberSearchViewModel model)
    {
        model.PageHeading = ApplicationLabels.SearchPupilPremiumWithUpnPageHeading;
        model.LearnerNumberLabel = Global.LearnerNumberLabel;
        model.ShowLocalAuthority = true;
        model.ShowMiddleNames = true;
        return model;
    }

    private LearnerNumberSearchViewModel PopulateNavigation(LearnerNumberSearchViewModel model)
    {
        model.DownloadLinksPartial = DownloadLinksPartial;
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

    private void SetModelApplicationLabels(LearnerNumberSearchViewModel model)
    {
        model.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
        model.DownloadSelectedLink = ApplicationLabels.DownloadSelectedPupilPremiumDataLink;
        model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
    }

    private static string GetLearnerIdsAsString(
    IEnumerable<Learner> learners) =>
        string.Join(",", learners.Select(learner => learner.Id));

    private static List<string> SetLearnerNumberIds(IEnumerable<Learner> learners)
    {
        List<string> idList = [];
        foreach (Learner learner in learners)
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

    private static HashSet<string> GetLearnerNumberIds(
    IEnumerable<Learner> learners)
    {
        HashSet<string> learnerNumberIds = [];

        foreach (Learner learner in learners)
        {
            learnerNumberIds.Add(learner.LearnerNumberId);
        }

        return learnerNumberIds;
    }


    // Stores sorting data into session to make it reachable on returnToSearch
    private void SetSortingDataIntoSession(string sortField, string sortDirection)
    {
        HttpContext.Session.SetString(SearchSessionSortField, sortField);
        HttpContext.Session.SetString(SearchSessionSortDirection, sortDirection);
    }

    private LearnerNumberSearchViewModel SetSortingDataIntoModel(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
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
    #endregion
}
