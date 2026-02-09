using System.Text.RegularExpressions;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Search.Options.Search;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Services.Download.CTF;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;

[Route(Routes.Application.Search)]
public sealed class NationalPupilDatabaseLearnerNumberSearchController : Controller
{
    private const int PAGESIZE = 20;
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";
    public const string TOTAL_SEARCH_RESULTS = "totalSearch";

    private readonly ILogger<NationalPupilDatabaseLearnerNumberSearchController> _logger;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IUseCase<NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse> _searchUseCase;
    private readonly ISortOrderFactory _sortOrderFactory;
    private readonly IMapper<NationalPupilDatabaseLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel> _learnerNumericSearchResponseToViewModelMapper;
    private readonly ISelectionManager _selectionManager;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    private readonly AzureAppSettings _appSettings;

    public string PageHeading => ApplicationLabels.SearchNPDWithUpnPageHeading;
    public string SearchAction => "NationalPupilDatabase";
    public string FullTextLearnerSearchController => "Search";
    public string FullTextLearnerSearchAction => Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;
    public string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchPageDownloadLinks.cshtml";
    public string SearchSessionKey => "SearchNPD_SearchText";
    public string SearchSessionSortField => "SearchNPD_SearchTextSortField";
    public string SearchSessionSortDirection => "SearchNPD_SearchTextSortDirection";
    public string DownloadSelectedLink => ApplicationLabels.DownloadSelectedNationalPupilDatabaseDataLink;

    private readonly IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> _getAvailableDatasetsForPupilsUseCase;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadPupilDataUseCase;
    private readonly IEventLogger _eventLogger;
    private readonly ISearchIndexOptionsProvider _searchIndexOptionsProvider;
    private readonly IMapper<SearchCriteriaOptions, SearchCriteria> _criteriaOptionsToCriteriaMapper;

    public NationalPupilDatabaseLearnerNumberSearchController(
        ILogger<NationalPupilDatabaseLearnerNumberSearchController> logger,
        IDownloadCommonTransferFileService ctfService,
        IUseCase<
            NationalPupilDatabaseSearchByUniquePupilNumberRequest, NationalPupilDatabaseSearchByUniquePupilNumberResponse> searchUseCase,
        ISortOrderFactory sortOrderFactory,
        IMapper<
            NationalPupilDatabaseLearnerNumericSearchMappingContext,
            LearnerNumberSearchViewModel> learnerNumericSearchResponseToViewModelMapper,
        ISelectionManager selectionManager,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> getAvailableDatasetsForPupilsUseCase,
        IJsonSerializer jsonSerializer,
        IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> downloadPupilDataUseCase,
        IEventLogger eventLogger,
        ISearchIndexOptionsProvider searchIndexOptionsProvider,
        IMapper<SearchCriteriaOptions, SearchCriteria> criteriaOptionsToCriteriaMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(searchUseCase);
        _searchUseCase = searchUseCase;

        ArgumentNullException.ThrowIfNull(sortOrderFactory);
        _sortOrderFactory = sortOrderFactory;

        ArgumentNullException.ThrowIfNull(learnerNumericSearchResponseToViewModelMapper);
        _learnerNumericSearchResponseToViewModelMapper = learnerNumericSearchResponseToViewModelMapper;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(getAvailableDatasetsForPupilsUseCase);
        _getAvailableDatasetsForPupilsUseCase = getAvailableDatasetsForPupilsUseCase;

        ArgumentNullException.ThrowIfNull(jsonSerializer);
        _jsonSerializer = jsonSerializer;

        ArgumentNullException.ThrowIfNull(downloadPupilDataUseCase);
        _downloadPupilDataUseCase = downloadPupilDataUseCase;

        ArgumentNullException.ThrowIfNull(eventLogger);
        _eventLogger = eventLogger;

        ArgumentNullException.ThrowIfNull(searchIndexOptionsProvider);
        _searchIndexOptionsProvider = searchIndexOptionsProvider;

        ArgumentNullException.ThrowIfNull(criteriaOptionsToCriteriaMapper);
        _criteriaOptionsToCriteriaMapper = criteriaOptionsToCriteriaMapper;
    }


    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber)]
    [HttpGet]
    public async Task<IActionResult> NationalPupilDatabase(bool? returnToSearch)
    {
        _logger.LogInformation("National pupil database Upn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NationalPupilDatabase(
        [FromForm] LearnerNumberSearchViewModel model,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("National pupil database Upn POST method called");

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
    [Route("add-npd-to-my-pupil-list")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NPDAddToMyPupilList(LearnerNumberSearchViewModel model)
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


    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadCommonTransferFileData(LearnerNumberSearchViewModel model)
    {
        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        string[] available = model.LearnerNumberIds.FormatLearnerNumbers();
        HashSet<string> selected = GetSelected(available);

        if (selected.Count == 0)
        {
            model.ErrorDetails = Messages.Downloads.Errors.NoPupilSelected;
            model.NoPupil = true;
            model.NoPupilSelected = true;
            return await NationalPupilDatabase(model, model.PageNumber, HttpContext.Session.GetString(SearchSessionSortField), HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        if (selected.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            model.ErrorDetails = Messages.Downloads.Errors.UPNLimitExceeded;
            return await NationalPupilDatabase(model, model.PageNumber, HttpContext.Session.GetString(SearchSessionSortField), HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        ReturnFile downloadFile = await _ctfService.GetCommonTransferFile([.. selected],
                                                                model.LearnerNumber.FormatLearnerNumbers(),
                                                                User.GetLocalAuthorityNumberForEstablishment(),
                                                                User.GetEstablishmentNumber(),
                                                                User.IsOrganisationEstablishment(),
                                                                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                                                                ReturnRoute.NationalPupilDatabase);

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }
        else
        {
            model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
        }

        return await NationalPupilDatabase(model, model.PageNumber, HttpContext.Session.GetString(SearchSessionSortField), HttpContext.Session.GetString(SearchSessionSortDirection), true);
    }

    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
        string selectedPupilsJoined,
        string learnerNumber,
        int selectedPupilsCount)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupilsJoined,
            LearnerNumber = learnerNumber,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        ModelState.Clear();

        PopulateNavigation(searchDownloadViewModel.NumberSearchViewModel);
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = searchDownloadViewModel.LearnerNumber;
        searchDownloadViewModel.SearchAction = Global.NPDAction;
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumberLabel = "UPN";

        string[] selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            GetAvailableDatasetsForPupilsRequest request = new(
             DownloadType: Core.Downloads.Application.Enums.DownloadType.NPD,
             SelectedPupils: selectedPupils,
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
        }
        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.DownloadNPDOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedPupils))
        {
            string[] selectedPupils = model.SelectedPupils.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                List<Core.Downloads.Application.Enums.Dataset> selectedDatasets = [];
                foreach (string datasetString in model.SelectedDownloadOptions)
                {
                    if (Enum.TryParse(datasetString, ignoreCase: true, out Core.Downloads.Application.Enums.Dataset dataset))
                        selectedDatasets.Add(dataset);
                }

                DownloadPupilDataRequest request = new(
                   SelectedPupils: selectedPupils,
                   SelectedDatasets: selectedDatasets,
                   DownloadType: Core.Downloads.Application.Enums.DownloadType.NPD,
                   FileFormat: model.DownloadFileType == DownloadFileType.CSV ? FileFormat.Csv : FileFormat.Tab);

                DownloadPupilDataResponse response = await _downloadPupilDataUseCase.HandleRequestAsync(request);

                string loggingBatchId = Guid.NewGuid().ToString();
                foreach (string dataset in model.SelectedDownloadOptions)
                {
                    // TODO: Temp quick solution
                    if (Enum.TryParse(dataset, out Core.Common.CrossCutting.Logging.Events.Dataset datasetEnum))
                    {
                        _eventLogger.LogDownload(
                            Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                            model.DownloadFileType == DownloadFileType.CSV ? DownloadFileFormat.CSV : DownloadFileFormat.TAB,
                            DownloadEventType.NPD,
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

            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }

        return RedirectToAction(Global.NPDLearnerNumberSearchAction, Global.NPDLearnerNumberSearchController);
    }

    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToDownloadSelectedNPDDataUPN(LearnerNumberSearchViewModel searchViewModel)
    {
        SetSelections(
            searchViewModel.PageLearnerNumbers.Split(','),
            searchViewModel.SelectedPupil);

        HashSet<string> selectedPupils = GetSelected(searchViewModel.LearnerNumberIds.FormatLearnerNumbers());

        if (selectedPupils.Count == 0)
        {
            searchViewModel.NoPupil = true;
            searchViewModel.NoPupilSelected = true;
            return await NationalPupilDatabase(searchViewModel, searchViewModel.PageNumber, HttpContext.Session.GetString(SearchSessionSortField), HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        string joinedSelectedPupils = string.Join(',', selectedPupils);
        return await DownloadSelectedNationalPupilDatabaseData(joinedSelectedPupils, searchViewModel.LearnerNumber, selectedPupils.Count);
    }

    #region WIP Refactor out inherited methods

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
        _logger.LogInformation("BaseLearnerNumberController POST method called");
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

            model = await GetPupilsForSearchBuilder(model, notPaged).ConfigureAwait(false);
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

    private async Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model)
    {
        return await NationalPupilDatabase(model, model.PageNumber, HttpContext.Session.GetString(SearchSessionSortField), HttpContext.Session.GetString(SearchSessionSortDirection), true);
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

        SearchIndexOptions options = _searchIndexOptionsProvider.GetOptions(key: "npd-upn");

        SortOrder sortOrder = _sortOrderFactory.Create(
            options: options.SortOptions, 
            sort: (model.SortField, model.SortDirection));

        NationalPupilDatabaseSearchByUniquePupilNumberResponse searchResponse = 
            await _searchUseCase.HandleRequestAsync(
                new NationalPupilDatabaseSearchByUniquePupilNumberRequest()
                {
                    UniquePupilNumbers = learnerNumberArray,
                    SearchCriteria = _criteriaOptionsToCriteriaMapper.Map(options.SearchCriteria),
                    Sort = sortOrder,
                    Offset = model.Offset
                });

        LearnerNumberSearchViewModel result =
            _learnerNumericSearchResponseToViewModelMapper.Map(
                NationalPupilDatabaseLearnerNumericSearchMappingContext.Create(model, searchResponse));

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

    private static HashSet<string> GetLearnerNumberIds(IEnumerable<Learner> learners)
    {
        HashSet<string> learnerNumberIds = [];

        foreach (Learner learner in learners)
        {
            learnerNumberIds.Add(learner.LearnerNumberId);
        }

        return learnerNumberIds;
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

    private LearnerNumberSearchViewModel PopulatePageText(LearnerNumberSearchViewModel model)
    {
        model.PageHeading = PageHeading;
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

    private static LearnerNumberSearchViewModel SetSortingDataIntoModel(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
    {
        model.SortField = sortField;
        model.SortDirection = sortDirection;
        return model;
    }

    // Stores sorting data into session to make it reachable on returnToSearch


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
