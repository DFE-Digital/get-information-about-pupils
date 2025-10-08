﻿using System.Text.RegularExpressions;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Search.FurtherEducation;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.LearnerNumber.Mappers;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Controllers.LearnerNumber;

[FeatureGate(FeatureFlags.FurtherEducation)]
[Route(Routes.Application.Search)]
public class FELearnerNumberController : Controller
{
    private readonly ILogger<FELearnerNumberController> _logger;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;
    protected readonly ISelectionManager _selectionManager;
    private readonly IUseCase<
        SearchRequest,
        SearchResponse> _furtherEducationSearchUseCase;
    private readonly IMapper<
        LearnerNumericSearchMappingContext,
        LearnerNumberSearchViewModel> _learnerNumericSearchResponseToViewModelMapper;

    public const int PAGESIZE = 20;
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";
    public const string TOTAL_SEARCH_RESULTS = "totalSearch";

    public string PageHeading => UniqueLearnerNumberLabels.SearchPupilUpnPageHeading;
    public string SearchAction => "PupilUlnSearch";
    public string FullTextLearnerSearchController => Global.FELearnerTextSearchController;
    public string FullTextLearnerSearchAction => Global.FELearnerTextSearchAction;
    public string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchFurtherEducationDownloadLinks.cshtml";
    public AzureSearchIndexType IndexType => AzureSearchIndexType.FurtherEducation;
    public string SearchSessionKey => "SearchULN_SearchText";
    public string SearchSessionSortField => "SearchULN_SearchTextSortField";
    public string SearchSessionSortDirection => "SearchULN_SearchTextSortDirection";
    public bool ShowLocalAuthority => false;
    public bool ShowMiddleNames => false;
    public string DownloadSelectedLink => ApplicationLabels.DownloadSelectedFurtherEducationLink;
    public string LearnerNumberLabel => Global.FELearnerNumberLabel;
    public string InvalidUPNsConfirmationAction => "";


    public FELearnerNumberController(
        IUseCase<
            SearchRequest,
            SearchResponse> furtherEducationSearchUseCase,
        IMapper<
            LearnerNumericSearchMappingContext,
            LearnerNumberSearchViewModel> learnerNumericSearchResponseToViewModelMapper,
        ILogger<FELearnerNumberController> logger,
        IDownloadService downloadService,
        ISelectionManager selectionManager,
        IOptions<AzureAppSettings> azureAppSettings)
    {
        _furtherEducationSearchUseCase = furtherEducationSearchUseCase ??
           throw new ArgumentNullException(nameof(furtherEducationSearchUseCase));
        _learnerNumericSearchResponseToViewModelMapper = learnerNumericSearchResponseToViewModelMapper ??
            throw new ArgumentNullException(nameof(learnerNumericSearchResponseToViewModelMapper));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
        _appSettings = azureAppSettings.Value;
        _selectionManager = selectionManager ??
            throw new ArgumentNullException(nameof(selectionManager));
    }

    [Route(Routes.FurtherEducation.LearnerNumberSearch)]
    [HttpGet]
    public async Task<IActionResult> PupilUlnSearch(bool? returnToSearch)
    {
        if (!(User.IsEstablishmentWithFurtherEducation() || User.IsEstablishmentWithAccessToULNPages() || User.IsDfeUser()))
        {
            return RedirectToAction("Error", "Home");
        }

        _logger.LogInformation("Pupil Uln Search GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.FurtherEducation.LearnerNumberSearch)]
    [HttpPost]
    public async Task<IActionResult> PupilUlnSearch(
        [FromForm] LearnerNumberSearchViewModel model,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        if (!(User.IsEstablishmentWithFurtherEducation() || User.IsEstablishmentWithAccessToULNPages() || User.IsDfeUser()))
        {
            return RedirectToAction("Error", "Home");
        }

        _logger.LogInformation("Pupil Unique Learner Number  Search POST method called");

        return await Search(
            model,
            pageNumber,
            sortField,
            sortDirection,
            !ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset")
        );
    }

    [Route(UniqueLearnerNumberLabels.ToDownloadSelectedULNData)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedULNData(LearnerNumberSearchViewModel searchViewModel)
    {
        SetSelections(
         searchViewModel.PageLearnerNumbers.Split(','),
         searchViewModel.SelectedPupil);

        HashSet<string> selectedPupils = GetSelected(searchViewModel.LearnerNumber.FormatLearnerNumbers());

        if (selectedPupils.Count == 0)
        {
            searchViewModel.NoPupil = true;
            searchViewModel.NoPupilSelected = true;
            return await PupilUlnSearch(
                searchViewModel, searchViewModel.PageNumber,
                HttpContext.Session.GetString(SearchSessionSortField),
                HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        string joinedSelectedPupils = string.Join(',', selectedPupils);
        return await DownloadSelectedUlnDatabaseData(joinedSelectedPupils, searchViewModel.LearnerNumber, selectedPupils.Count);
    }

    [Route(UniqueLearnerNumberLabels.DownloadSelectedUlnData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedUlnDatabaseData(LearnerDownloadViewModel model)
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
                ReturnFile downloadFile =
                    await _downloadService.GetFECSVFile(
                        selectedPupils,
                        model.SelectedDownloadOptions,
                        true,
                        AzureFunctionHeaderDetails.Create(
                            User.GetUserId(), User.GetSessionId()), ReturnRoute.UniqueLearnerNumber).ConfigureAwait(false);

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

            return await DownloadSelectedUlnDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }
        return RedirectToAction(SearchAction, UniqueLearnerNumberLabels.SearchUlnControllerName);
    }

    [Route(UniqueLearnerNumberLabels.DownloadSelectedUlnData)]
    [HttpGet]
    public async Task<IActionResult> DownloadSelectedUlnDatabaseData(string selectedPupilsJoined, string uln, int selectedPupilsCount)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupilsJoined,
            LearnerNumber = uln,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = false
        };

        SearchDownloadHelper.AddUlnDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationHighAge(), User.IsDfeUser());
        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumULNsPerSearch;
        ModelState.Clear();

        PopulateNavigation(searchDownloadViewModel.NumberSearchViewModel);
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = SearchAction;
        searchDownloadViewModel.DownloadRoute = UniqueLearnerNumberLabels.DownloadSelectedUlnData;
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumberLabel = "ULN";

        string[] selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            string[] downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            IEnumerable<DownloadUlnDataType> disabledTypes =
                await _downloadService.CheckForFENoDataAvailable(
                    selectedPupils,
                    downloadTypeArray,
                    AzureFunctionHeaderDetails.Create(
                        User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }
        searchDownloadViewModel.SearchResultPageHeading = PageHeading;

        //need to update searchbox to show uln wording
        return View(Global.DownloadNPDOptionsView, searchDownloadViewModel);
    }

    protected bool ValidateLearnerNumber(string learnerNumber)
    {
        return ValidationHelper.IsValidUln(learnerNumber);
    }

    protected string GenerateValidationMessage()
    {
        return PupilHelper.GenerateValidationMessageUlnSearch(ModelState);
    }

    #region WIP - this will be slowly refactored away from the controller as we move through more search types

    [NonAction]
    public async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerNumberSearchViewModel model = new();

        PopulatePageText(model);
        PopulateNavigation(model);
        PopulateSorting(model, HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection));
        ClearSortingDataFromSession();
        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;

        model.ShowMiddleNames = ShowMiddleNames;

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

        model.ShowMiddleNames = ShowMiddleNames;

        model.SearchBoxErrorMessage = ModelState.IsValid is false ? GenerateValidationMessage() : null;

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

            model = await GetPupilsForSearchBuilder(
                model,
                IndexType,
                pageNumber,
                notPaged).ConfigureAwait(false);
            model.PageNumber = pageNumber;
            model.PageSize = PAGESIZE;
            model.MaximumResults = _appSettings.MaximumULNsPerSearch;
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

    #region Invalid Numbers

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

                case Global.InvalidUPNConfirmation_MyPupilList:
                    return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
            }
        }
        else
        {
            ModelState.AddModelError("NoContinueSelection", Messages.Common.Errors.NoContinueSelection);
        }

        return await InvalidUPNs(model);
    }

    #endregion Invalid Numbers

    #region Private Methods

    private async Task<LearnerNumberSearchViewModel> GetPupilsForSearchBuilder(
        LearnerNumberSearchViewModel model,
        AzureSearchIndexType indexType,
        int pageNumber,
        bool first)
    {
        if (string.IsNullOrEmpty(model.LearnerNumber)) return model;

        string[] learnerNumberArray = model.LearnerNumber.FormatLearnerNumbers();

        string searchText = model.LearnerNumber.ToSearchText();

        if (!first)
        {
            searchText = model.LearnerIdSearchResult;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        model.MaximumResults = _appSettings.MaximumUPNsPerSearch;
        
        SortOrder sortOrder = new(sortField: "Forename", sortDirection: "desc", ["Forename", "Surname"]);

        IList<FilterRequest> filterRequests =
        [
            new FilterRequest(filterName: "ULN", learnerNumberArray)
        ];

        SearchResponse searchResponse =
            await _furtherEducationSearchUseCase.HandleRequestAsync(
                new SearchRequest(
                    searchKeywords: string.Join(" AND ", learnerNumberArray),
                    filterRequests: filterRequests,
                    sortOrder: sortOrder,
                    offset: model.Offset))
            .ConfigureAwait(false);

        LearnerNumberSearchViewModel result =
            _learnerNumericSearchResponseToViewModelMapper.Map(
                LearnerNumericSearchMappingContext.Create(model, searchResponse));

        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        List<string> idList = SetLearnerNumberIds(result.Learners);

        string[] combinedIdLearnerNumberArray = [.. learnerNumberArray, .. idList];

        if (first)
        {
            model.LearnerIdSearchResult = GetLearnerIdsAsString(result.Learners);
            HashSet<string> learnerNumberIdSet = GetLearnerNumberIds(result.Learners);
            HashSet<string> learnerNumberSet = GetLearnerNumbers(result.Learners);
            model.LearnerNumberIds = string.Join("\n", learnerNumberIdSet);
            IEnumerable<string> missing = combinedIdLearnerNumberArray.Except(learnerNumberIdSet).Except(learnerNumberSet);

            HttpContext.Session.SetString(MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(missing));
            HttpContext.Session.SetString(TOTAL_SEARCH_RESULTS, model.Total.ToString());
        }
        else
        {
            model.Total = Convert.ToInt32(HttpContext.Session.GetString(TOTAL_SEARCH_RESULTS));
        }

        model.NotFound = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));

        List<string> duplicateLearnerNumbers = ValidationHelper.GetDuplicates([.. learnerNumberArray]);

        if (duplicateLearnerNumbers.Count != 0)
        {
            foreach (string learnerNumber in duplicateLearnerNumbers)
            {
                model.Duplicates.Add(learnerNumber);
            }
        }

        List<string> potentialErrorLearnerNumbers = learnerNumberArray.Distinct().ToList();

        if (potentialErrorLearnerNumbers.Count != 0)
        {
            foreach (string learnerNumber in potentialErrorLearnerNumbers)
            {
                bool isValid = ValidateLearnerNumber(learnerNumber);

                if (!isValid)
                {
                    model.Invalid.Add(learnerNumber);
                }
            }

            _logger.LogError("Some of the LearnerNumber(s) have not been found in our database");
        }

        // ensure that the selections are set appropriately
        HashSet<string> selected = GetSelected(combinedIdLearnerNumberArray);
        foreach (Domain.Search.Learner.Learner learner in result.Learners)
        {
            learner.Selected = selected.Contains(learner.LearnerNumberId);
        }

        model.Learners = first ? result.Learners.Take(PAGESIZE) : result.Learners;
        model.PageLearnerNumbers = string.Join(',', model.Learners.Select(l => l.LearnerNumberId));
        return model;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <param name="indexType"></param>
    /// <returns></returns>
    private async Task<InvalidLearnerNumberSearchViewModel> GetInvalidPupils(
        InvalidLearnerNumberSearchViewModel model, AzureSearchIndexType indexType)
    {
        if (string.IsNullOrEmpty(model.LearnerNumber)) return model;

        //string searchInput = model.LearnerNumber.ToDecryptedSearchText();

        //var result = await _paginatedSearch.GetPage(
        //  searchInput,
        //    null,
        //     _appSettings.MaximumUPNsPerSearch,
        //    0,
        //    indexType,
        //    AzureSearchQueryType.Numbers,
        //    AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
        //    );

        //model.Learners = result.Learners ?? new List<Learner>();

        //var nonUPNResult = await _paginatedSearch.GetPage(
        //searchInput,
        //null,
        // _appSettings.MaximumUPNsPerSearch,
        //0,
        //indexType,
        //AzureSearchQueryType.Id,
        //AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
        //);

        //model.Learners = model.Learners.Union(nonUPNResult.Learners);

        return model;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="learners"></param>
    /// <returns></returns>
    private static List<string> SetLearnerNumberIds(
        IEnumerable<Domain.Search.Learner.Learner> learners)
    {
        List<string> idList = [];
        foreach (Domain.Search.Learner.Learner learner in learners)
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
            };
        }

        return idList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="learners"></param>
    /// <returns></returns>
    private static HashSet<string> GetLearnerNumbers(
        IEnumerable<Domain.Search.Learner.Learner> learners)
    {
        HashSet<string> learnerNumbers = [];

        foreach (Domain.Search.Learner.Learner learner in learners)
        {
            learnerNumbers.Add(learner.LearnerNumber);
        }

        return learnerNumbers;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="learners"></param>
    /// <returns></returns>
    private static HashSet<string> GetLearnerNumberIds(
        IEnumerable<Domain.Search.Learner.Learner> learners)
    {
        HashSet<string> learnerNumberIds = [];

        foreach (Domain.Search.Learner.Learner learner in learners)
        {
            learnerNumberIds.Add(learner.LearnerNumberId);
        }

        return learnerNumberIds;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="learners"></param>
    /// <returns></returns>
    private static string GetLearnerIdsAsString(
        IEnumerable<Domain.Search.Learner.Learner> learners) =>
            string.Join(",", learners.Select(learner => learner.Id));

    #endregion Private Methods

    #region Protected Methods

    protected HashSet<string> GetSelected(string[] available)
    {
        // ensure we remove the missing items
        var missing = JsonConvert.DeserializeObject<List<string>>(this.HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));

        if (missing != null)
        {
            var actuallyAvailable = available.Except(missing).ToArray();
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

    protected LearnerNumberSearchViewModel PopulatePageText(LearnerNumberSearchViewModel model)
    {
        model.PageHeading = PageHeading;
        model.LearnerNumberLabel = LearnerNumberLabel;
        model.ShowLocalAuthority = ShowLocalAuthority;

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
        else if (!string.IsNullOrEmpty(this.HttpContext.Session.GetString(SearchSessionSortField)) && !string.IsNullOrEmpty(this.HttpContext.Session.GetString(SearchSessionSortDirection)))
            SetSortingDataIntoModel(model, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection));
        return model;
    }

    // Stores sorting data into session to make it reachable on returnToSearch
    protected void SetSortingDataIntoSession(string sortField, string sortDirection)
    {
        this.HttpContext.Session.SetString(SearchSessionSortField, sortField);
        this.HttpContext.Session.SetString(SearchSessionSortDirection, sortDirection);
    }

    protected LearnerNumberSearchViewModel SetSortingDataIntoModel(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
    {
        model.SortField = sortField;
        model.SortDirection = sortDirection;
        return model;
    }

    private void ClearSortingDataFromSession()
    {
        if (this.HttpContext.Session.Keys.Contains(SearchSessionSortField) && this.HttpContext.Session.Keys.Contains(SearchSessionSortDirection))
        {
            this.HttpContext.Session.Remove(SearchSessionSortField);
            this.HttpContext.Session.Remove(SearchSessionSortDirection);
        }
    }

    #endregion Protected Methods

    #endregion
}
