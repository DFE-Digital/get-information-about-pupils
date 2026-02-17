using System.Text.RegularExpressions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;

[Route(Routes.Application.Search)]
public class PupilPremiumLearnerNumberSearchController : Controller
{
    private const int PAGESIZE = 20;
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";
    public const string TOTAL_SEARCH_RESULTS = "totalSearch";

    private readonly ILogger<PupilPremiumLearnerNumberSearchController> _logger;

    private readonly IUseCase<
        PupilPremiumSearchByUniquePupilNumberRequest,
        SearchResponse<PupilPremiumLearners>> _searchUseCase;

    private readonly IMapper<
        PupilPremiumLearnerNumericSearchMappingContext,
        LearnerNumberSearchViewModel> _learnerNumericSearchResponseToViewModelMapper;

    private readonly ISelectionManager _selectionManager;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;
    private readonly ISearchIndexOptionsProvider _searchIndexOptionsProvider;
    private readonly IMapper<SearchCriteriaOptions, SearchCriteria> _criteriaOptionsToCriteriaMapper;
    private readonly ISortOrderFactory _sortOrderFactory;

    public string SearchAction => nameof(PupilPremium);
    public string FullTextLearnerSearchController => Global.PPTextSearchController;
    public string FullTextLearnerSearchAction => "NonUpnPupilPremiumDatabase";
    public string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchPupilPremiumDownloadLinks.cshtml";
    public string SearchSessionKey => "SearchPPUPN_SearchText";
    public string SearchSessionSortField => "SearchPPUPN_SearchTextSortField";
    public string SearchSessionSortDirection => "SearchPPUPN_SearchTextSortDirection";

    public PupilPremiumLearnerNumberSearchController(
        ILogger<PupilPremiumLearnerNumberSearchController> logger,
        IUseCase<
            PupilPremiumSearchByUniquePupilNumberRequest,
            SearchResponse<PupilPremiumLearners>> searchUseCase,
        ISortOrderFactory sortOrderFactory,
        IMapper<
            PupilPremiumLearnerNumericSearchMappingContext,
            LearnerNumberSearchViewModel> learnerNumericSearchResponseToViewModelMapper,
        ISelectionManager selectionManager,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IJsonSerializer jsonSerializer,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService,
        ISearchIndexOptionsProvider searchIndexOptionsProvider,
        IMapper<SearchCriteriaOptions, SearchCriteria> criteriaOptionsToCriteriaMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(searchUseCase);
        _searchUseCase = searchUseCase;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;

        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupilsService);
        _downloadPupilPremiumDataForPupilsService = downloadPupilPremiumDataForPupilsService;

        ArgumentNullException.ThrowIfNull(searchIndexOptionsProvider);
        _searchIndexOptionsProvider = searchIndexOptionsProvider;

        ArgumentNullException.ThrowIfNull(criteriaOptionsToCriteriaMapper);
        _criteriaOptionsToCriteriaMapper = criteriaOptionsToCriteriaMapper;

        ArgumentNullException.ThrowIfNull(sortOrderFactory);
        _sortOrderFactory = sortOrderFactory;

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
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PPAddToMyPupilList(LearnerNumberSearchViewModel model)
    {
        PopulatePageText(model);
        PopulateNavigation(model);

        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        HashSet<string> selectedUpns = GetSelected(model.LearnerNumberIds.FormatLearnerNumbers());

        if (selectedUpns.Count == 0)
        {
            model.NoPupil = true;
            model.NoPupilSelected = true;
            return await ReturnToPage(model);
        }

        List<string> invalidUPNs = selectedUpns.Where(x => !ValidationHelper.IsValidUpn(x)).ToList();

        if (invalidUPNs.Count > 0)
        {
            model.Invalid = invalidUPNs;
            model.ErrorDetails = Messages.Common.Errors.InvalidPupilIdentifier;
            model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);
        }

        model.SelectedPupil = model.SelectedPupil.Except(invalidUPNs).ToList();
        selectedUpns = selectedUpns.Where(t => !invalidUPNs.Contains(t)).ToHashSet();

        try
        {
            await _addPupilsToMyPupilsUseCase.HandleRequestAsync(
                new AddPupilsToMyPupilsRequest(
                    userId: User.GetUserId(),
                    pupils: selectedUpns));
        }

        catch (MyPupilsLimitExceededException)
        {
            model.ErrorDetails = Messages.Common.Errors.MyPupilListLimitExceeded;
            return await ReturnToPage(model);
        }

        model.ItemAddedToMyPupilList = true;
        return await ReturnToPage(model);
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

        SetModelApplicationLabels(model);

        if (returnToSearch ?? false && HttpContext.Session.Keys.Contains(SearchSessionKey))
        {
            ModelState.Clear();
            model.LearnerNumber = HttpContext.Session.GetString(SearchSessionKey);
            model = await GetPupilsForSearchBuilder(model, true).ConfigureAwait(false);
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

        SearchIndexOptions options = _searchIndexOptionsProvider.GetOptions(key: "pupil-premium-upn");

        SortOrder sortOrder = _sortOrderFactory.Create(
            options.SortOptions,
            sort: (model.SortField, model.SortDirection));

        SearchResponse<PupilPremiumLearners> searchResponse = await _searchUseCase.HandleRequestAsync(
            new PupilPremiumSearchByUniquePupilNumberRequest()
            {
                UniquePupilNumbers = learnerNumberArray,
                SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(options.SearchCriteria),
                Sort = sortOrder,
                Offset = model.Offset
            });

        LearnerNumberSearchViewModel result =
            _learnerNumericSearchResponseToViewModelMapper.Map(
                PupilPremiumLearnerNumericSearchMappingContext.Create(model, searchResponse));

        List<string> idList = SetLearnerNumberIds(result.Learners);

        string[] combinedIdLearnerNumberArray = [.. learnerNumberArray, .. idList];

        if (first)
        {
            model.LearnerIdSearchResult = string.Join(",", result.Learners.Select(learner => learner.Id));
            HashSet<string> learnerNumberIdSet = GetLearnerNumberIds(result.Learners);
            model.LearnerNumberIds = string.Join("\n", learnerNumberIdSet);

            IEnumerable<string> missing = combinedIdLearnerNumberArray.Except(learnerNumberIdSet);

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

        List<string> duplicateLearnerNumbers = ValidationHelper.GetDuplicates([.. learnerNumberArray]);

        if (duplicateLearnerNumbers.Count > 0)
        {
            foreach (string learnerNumber in duplicateLearnerNumbers)
            {
                model.Duplicates.Add(learnerNumber);
            }
        }

        List<string> potentialErrorLearnerNumbers = learnerNumberArray.Distinct().ToList();

        if (potentialErrorLearnerNumbers.Count > 0)
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
        // store sorting in session so returnToSearch can retrieve
        if (!string.IsNullOrEmpty(sortField) && !string.IsNullOrEmpty(sortDirection))
        {
            HttpContext.Session.SetString(SearchSessionSortField, sortField);
            HttpContext.Session.SetString(SearchSessionSortDirection, sortDirection);
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


    private static LearnerNumberSearchViewModel PopulatePageText(LearnerNumberSearchViewModel model)
    {
        model.PageHeading = ApplicationLabels.SearchPupilPremiumWithUpnPageHeading;
        model.LearnerNumberLabel = Global.LearnerNumberLabel;
        model.ShowLocalAuthority = true;
        model.ShowMiddleNames = true;
        return model;
    }

    private static void SetModelApplicationLabels(LearnerNumberSearchViewModel model)
    {
        model.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
        model.DownloadSelectedLink = ApplicationLabels.DownloadSelectedPupilPremiumDataLink;
        model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
    }

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

    private static LearnerNumberSearchViewModel SetSortingDataIntoModel(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
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
