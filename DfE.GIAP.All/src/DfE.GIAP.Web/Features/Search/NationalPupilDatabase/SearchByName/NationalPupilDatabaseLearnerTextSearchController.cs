using System.Text.Json;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.Controllers;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Services.Download.CTF;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using DownloadType = DfE.GIAP.Common.Enums.DownloadType;

namespace DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;

[Route(Routes.Application.Search)]
public sealed class NationalPupilDatabaseLearnerTextSearchController : Controller
{
    private const int PAGESIZE = 20;
    private const string PersistedSelectedSexFiltersKey = "PersistedSelectedSexFilters";
    private readonly ILogger<NationalPupilDatabaseLearnerTextSearchController> _logger;
    private readonly ITextSearchSelectionManager _selectionManager;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly ISessionProvider _sessionProvider;

    public string PageHeading => ApplicationLabels.SearchNPDWithOutUpnPageHeading;
    public string SearchSessionKey => Global.NPDNonUpnSearchSessionKey;
    public string SearchFiltersSessionKey => Global.NPDNonUpnSearchFiltersSessionKey;
    public string DownloadLinksPartial => Global.NPDNonUpnDownloadLinksView;

    public string SortDirectionKey => Global.NPDNonUpnSortDirectionSessionKey;
    public string SortFieldKey => Global.NPDNonUpnSortFieldSessionKey;

    public string SearchLearnerNumberController => Routes.Application.Search;
    public string SearchLearnerNumberAction => Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber;
    public string RedirectUrlFormAction => Global.NPDNonUpnAction;
    public string LearnerTextDatabaseName => Global.NPDLearnerTextSearchDatabaseName;
    public string RedirectFrom => Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;

    public string SearchView => Global.NonUpnSearchView;
    public string LearnerNumberLabel => Global.LearnerNumberLabel;
    public string SurnameFilterUrl => Routes.NationalPupilDatabase.NonUpnSurnameFilter;
    public string DobFilterUrl => Routes.NationalPupilDatabase.NonUpnDobFilter;
    public string ForenameFilterUrl => Routes.NationalPupilDatabase.NonUpnForenameFilter;
    public string MiddlenameFilterUrl => Routes.NationalPupilDatabase.NonUpnMiddlenameFilter;
    public string SexFilterUrl => Routes.NationalPupilDatabase.NonUpnSexFilter;
    public string FormAction => Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;
    public string RemoveActionUrl => $"/{Routes.Application.Search}/{Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN}";


    public string SearchAction => Global.NPDNonUpnAction;
    public string SearchController => Global.NPDTextSearchController;

    private readonly IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> _getAvailableDatasetsForPupilsUseCase;
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsToMyPupilsUseCase;
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadPupilDataUseCase;
    private readonly IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse> _downloadPupilCtfUseCase;
    private readonly IEventLogger _eventLogger;

    private readonly
        IUseCase<
            NationalPupilDatabaseSearchByNameRequest, NationalPupilDatabaseSearchByNameResponse> _searchUseCase;

    private readonly
        IMapper<
            NationalPupilDatabaseLearnerTextSearchMappingContext, LearnerTextSearchViewModel> _learnerSearchResponseToViewModelMapper;

    private readonly IMapper<
        Dictionary<string, string[]>, IList<FilterRequest>> _filtersRequestMapper;

    private readonly ISortOrderFactory _sortOrderFactory;

    private readonly IFiltersRequestFactory _filtersRequestBuilder;
    private readonly ISearchIndexOptionsProvider _searchIndexOptionsProvider;
    private readonly IMapper<SearchCriteriaOptions, SearchCriteria> _criteriaOptionsToCriteriaMapper;

    public NationalPupilDatabaseLearnerTextSearchController(ILogger<NationalPupilDatabaseLearnerTextSearchController> logger,
        ITextSearchSelectionManager selectionManager,
        IDownloadCommonTransferFileService ctfService,
        ISessionProvider sessionProvider,
        IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> getAvailableDatasetsForPupilsUseCase,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> downloadPupilDataUseCase,
        IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse> downloadPupilCtfUseCase,
        IEventLogger eventLogger,
        IUseCase<NationalPupilDatabaseSearchByNameRequest, NationalPupilDatabaseSearchByNameResponse> searchUseCase,
        IMapper<NationalPupilDatabaseLearnerTextSearchMappingContext, LearnerTextSearchViewModel> learnerSearchResponseToViewModelMapper,
        IMapper<Dictionary<string, string[]>, IList<FilterRequest>> filtersRequestMapper,
        ISortOrderFactory sortOrderFactory,
        IFiltersRequestFactory filtersRequestBuilder,
        ISearchIndexOptionsProvider searchIndexOptionsProvider,
        IMapper<SearchCriteriaOptions, SearchCriteria> criteriaOptionsToCriteriaMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(getAvailableDatasetsForPupilsUseCase);
        _getAvailableDatasetsForPupilsUseCase = getAvailableDatasetsForPupilsUseCase;

        ArgumentNullException.ThrowIfNull(addPupilsToMyPupilsUseCase);
        _addPupilsToMyPupilsUseCase = addPupilsToMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(downloadPupilDataUseCase);
        _downloadPupilDataUseCase = downloadPupilDataUseCase;

        ArgumentNullException.ThrowIfNull(downloadPupilCtfUseCase);
        _downloadPupilCtfUseCase = downloadPupilCtfUseCase;

        ArgumentNullException.ThrowIfNull(eventLogger);
        _eventLogger = eventLogger;

        ArgumentNullException.ThrowIfNull(searchUseCase);
        _searchUseCase = searchUseCase;

        ArgumentNullException.ThrowIfNull(learnerSearchResponseToViewModelMapper);
        _learnerSearchResponseToViewModelMapper = learnerSearchResponseToViewModelMapper;

        ArgumentNullException.ThrowIfNull(filtersRequestMapper);
        _filtersRequestMapper = filtersRequestMapper;

        ArgumentNullException.ThrowIfNull(sortOrderFactory);
        _sortOrderFactory = sortOrderFactory;

        ArgumentNullException.ThrowIfNull(filtersRequestBuilder);
        _filtersRequestBuilder = filtersRequestBuilder;

        ArgumentNullException.ThrowIfNull(searchIndexOptionsProvider);
        _searchIndexOptionsProvider = searchIndexOptionsProvider;

        ArgumentNullException.ThrowIfNull(criteriaOptionsToCriteriaMapper);
        _criteriaOptionsToCriteriaMapper = criteriaOptionsToCriteriaMapper;
    }

    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN)]
    [HttpGet]
    public async Task<IActionResult> NonUpnNationalPupilDatabase(bool? returnToSearch)
    {
        _logger.LogInformation("National pupil database NonUpn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN)]
    [HttpPost]
    public async Task<IActionResult> NonUpnNationalPupilDatabase(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("National pupil database NonUpn POST method called");
        model.ShowHiddenUPNWarningMessage = true;

        return await Search(
            model,
            surnameFilter,
            middlenameFilter,
            forenameFilter,
            searchByRemove,
            model.PageNumber,
            ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController,
            sortField,
            sortDirection,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [Route(Routes.NationalPupilDatabase.NonUpnDobFilter)]
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

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnSurnameFilter)]
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

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnMiddlenameFilter)]
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

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnForenameFilter)]
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

        return await ReturnToRoute(model).ConfigureAwait(false);
    }

    [Route(Routes.NationalPupilDatabase.NonUpnSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        SetPersistedSexFiltersForViewModel(model);
        ModelState.Clear();
        return await ReturnToRoute(model).ConfigureAwait(false);
    }


    [HttpPost]
    [Route(Routes.NPDNonUpnAddToMyPupilList)]
    public async Task<IActionResult> NonUpnAddToMyPupilList(LearnerTextSearchViewModel model)
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


    [Route(Routes.NationalPupilDatabase.DownloadCTFData)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadNpdCommonTransferFileData(LearnerTextSearchViewModel model)
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
            model.StarredPupilConfirmationViewModel.DownloadType = Common.Enums.DownloadType.CTF;
            model.StarredPupilConfirmationViewModel.SelectedPupil = selectedPupil;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        model.SelectedPupil = selectedPupil;

        return await DownloadNpdCommonTransferFileData(model);
    }

    private async Task<IActionResult> DownloadNpdCommonTransferFileData(LearnerTextSearchViewModel model)
    {
        string selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupil) ? RbacHelper.DecodeUpn(model.SelectedPupil) : model.SelectedPupil;

        DownloadPupilCtfRequest request = new(
            SelectedPupils: [selectedPupil],
            IsEstablishment: User.IsOrganisationEstablishment(),
            LocalAuthoriyNumber: User.GetLocalAuthorityNumberForEstablishment(),
            EstablishmentNumber: User.GetEstablishmentNumber());
        DownloadPupilCtfResponse response = await _downloadPupilCtfUseCase.HandleRequestAsync(request);

        if (response.FileContents is not null)
            return File(response.FileContents, response.ContentType, response.FileName);


        ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(new string[] { selectedPupil },
                                                                new string[] { ValidationHelper.IsValidUpn(selectedPupil) ? selectedPupil : "0" },
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

        return await ReturnToSearch(model);
    }

    [Route(Routes.NationalPupilDatabase.DownloadNonUPNConfirmationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationError = !model.ConfirmationGiven;
        PopulateConfirmationNavigation(model);

        if (model.ConfirmationGiven)
        {
            switch (model.DownloadType)
            {
                case DownloadType.CTF: return await DownloadNpdCommonTransferFileData(new LearnerTextSearchViewModel() { SelectedPupil = model.SelectedPupil });
                case DownloadType.NPD: return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupil, HttpContext.Session.Keys.Contains(SearchSessionKey) ? HttpContext.Session.GetString(SearchSessionKey) : string.Empty);
            }
        }

        return ConfirmationForStarredPupil(model);
    }

    [Route(Routes.NationalPupilDatabase.DownloadCancellationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadCancellationReturn()
    {
        return await Search(true);
    }

    private void PopulateConfirmationNavigation(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationReturnController = SearchController;
        model.ConfirmationReturnAction = Global.NPDDownloadConfirmationReturnAction;
        model.CancelReturnController = SearchController;
        model.CancelReturnAction = Global.NPDDownloadCancellationReturnAction;
    }

    [Route(Routes.NationalPupilDatabase.LearnerTextDataDownloadRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedNPDDataNonUPN(LearnerTextSearchViewModel model)
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
            model.StarredPupilConfirmationViewModel.DownloadType = DownloadType.NPD;
            model.StarredPupilConfirmationViewModel.SelectedPupil = selectedPupil;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        return await DownloadSelectedNationalPupilDatabaseData(selectedPupil, model.SearchText);
    }

    [Route(Routes.NationalPupilDatabase.LearnerTextDownloadOptions)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
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
            ShowTABDownloadType = true
        };

        ModelState.Clear();

        searchDownloadViewModel.LearnerNumber = selectedPupil;
        searchDownloadViewModel.SearchAction = Global.NPDAction;
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerTextDownloadFile;
        searchDownloadViewModel.RedirectRoute = Routes.NationalPupilDatabase.NationalPupilDatabaseNonUPN;
        searchDownloadViewModel.TextSearchViewModel = new LearnerTextSearchViewModel() { LearnerNumberLabel = LearnerNumberLabel, SearchText = searchText };
        PopulateNavigation(searchDownloadViewModel.TextSearchViewModel);

        GetAvailableDatasetsForPupilsRequest request = new(
           DownloadType: Core.Downloads.Application.Enums.DownloadType.NPD,
           SelectedPupils: new List<string> { selectedPupil },
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

        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.NonLearnerNumberDownloadOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.NationalPupilDatabase.LearnerTextDownloadFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedPupils))
        {
            string selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupils) ? RbacHelper.DecodeUpn(model.SelectedPupils) : model.SelectedPupils;

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
                    {
                        selectedDatasets.Add(dataset);
                    }
                }

                DownloadPupilDataRequest request = new(
                   SelectedPupils: [selectedPupil],
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

            if (HttpContext.Session.Keys.Contains(SearchSessionKey))
            {
                model.TextSearchViewModel.SearchText = HttpContext.Session.GetString(SearchSessionKey);
            }    

            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.TextSearchViewModel.SearchText);
        }

        return RedirectToAction(SearchAction, SearchController);
    }

    #region WIP inherited legacy methods


    private async Task<IActionResult> Search(bool? returnToSearch)
    {
        LearnerTextSearchViewModel model = new();

        PopulatePageText(model);
        PopulateNavigation(model);
        model.LearnerNumberLabel = LearnerNumberLabel;
        model.PageSize = PAGESIZE;

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
            model.SelectedSexValues = null;
            SetPersistedSexFiltersForViewModel(model);
            TempData.Remove(PersistedSelectedSexFiltersKey);
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

        model.ReturnRoute = ReturnRoute.NonNationalPupilDatabase;

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

    private async Task<IActionResult> ReturnToRoute(LearnerTextSearchViewModel model)
    {
        _selectionManager.Clear();
        ClearSortOptions();

        return await Search(model, null, null, null, null, model.PageNumber, false);
    }

    private void GetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model)
    {
        string[] sexFilters =
            TempData.GetPersistedObject<string[]>(
                PersistedSelectedSexFiltersKey,
                keepTempDataBetweenRequests: true);

        if (sexFilters != null)
        {
            model.SelectedSexValues = sexFilters;
        }
    }

    private void SetPersistedSexFiltersForViewModel(
        LearnerTextSearchViewModel model) =>
        TempData.SetPersistedObject(
            model.SelectedSexValues,
            PersistedSelectedSexFiltersKey);


    private IActionResult ConfirmationForStarredPupil(StarredPupilConfirmationViewModel model)
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

    private async Task<LearnerTextSearchViewModel> GenerateLearnerTextSearchViewModel(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string foremameFilter,
        string searchByRemove,
        string sortField = "",
        string sortDirection = "")
    {
        List<CurrentFilterDetail> currentFilters = SetCurrentFilters(model, surnameFilter, middlenameFilter, foremameFilter, searchByRemove);

        IList<FilterRequest> filterRequests =
            _filtersRequestMapper.Map(
                _filtersRequestBuilder
                    .GenerateFilterRequest(model, currentFilters));

        SearchIndexOptions options = _searchIndexOptionsProvider.GetOptions(key: "npd-text");

        SearchCriteria searchCriteria = _criteriaOptionsToCriteriaMapper.Map(options.SearchCriteria);

        SortOrder sortOrder =
            _sortOrderFactory.Create(
                options: options.SortOptions, 
                sort: (sortField, sortDirection));

        NationalPupilDatabaseSearchByNameResponse searchResponse =
            await _searchUseCase.HandleRequestAsync(
                new NationalPupilDatabaseSearchByNameRequest()
                {
                    SearchKeywords = model.SearchText,
                    SearchCriteria = searchCriteria,
                    SortOrder = sortOrder,
                    FilterRequests = filterRequests,
                    Offset = model.Offset
                });

        LearnerTextSearchViewModel viewModel = _learnerSearchResponseToViewModelMapper.Map(
            NationalPupilDatabaseLearnerTextSearchMappingContext.Create(model, searchResponse));


        foreach (Learner learner in viewModel.Learners)
        {
            learner.LearnerNumberId = learner.LearnerNumber switch
            {
                "0" => learner.Id,
                _ => learner.LearnerNumber,
            };
        }

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
            foreach (Learner learner in viewModel.Learners.Where((t) => !string.IsNullOrEmpty(t.LearnerNumberId)))
            {

                learner.Selected = selected.Contains(learner.LearnerNumberId);
            }
        }

        viewModel.LearnerTextDatabaseName = LearnerTextDatabaseName;

        viewModel.RedirectUrls.SurnameFilterURL = SurnameFilterUrl;
        viewModel.RedirectUrls.FormAction = FormAction;
        viewModel.RedirectUrls.RemoveAction = RemoveActionUrl;
        viewModel.RedirectUrls.DobFilterUrl = DobFilterUrl;
        viewModel.RedirectUrls.ForenameFilterUrl = ForenameFilterUrl;
        viewModel.RedirectUrls.MiddlenameFilterUrl = MiddlenameFilterUrl;
        viewModel.RedirectUrls.SexFilterUrl = SexFilterUrl;

        if (ModelState.IsValid)
        {
            viewModel.AddSelectedToMyPupilListLink = ApplicationLabels.AddSelectedToMyPupilListLink;
            viewModel.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
            viewModel.DownloadSelectedLink = ApplicationLabels.DownloadSelectedNationalPupilDatabaseDataLink;


            if (currentFilters != null)
            {
                if (currentFilters.Count > 0)
                {
                    viewModel.SearchFilters.CurrentFiltersApplied = currentFilters;
                }

                viewModel.SearchFilters.CurrentFiltersAppliedString = JsonSerializer.Serialize(currentFilters);
            }
        }

        viewModel.PageLearnerNumbers = string.Join(',', viewModel.Learners.Select(l => l.LearnerNumberId));

        viewModel.ShowOverLimitMessage = viewModel.Total > 100000;

        return viewModel;
    }

    private static List<CurrentFilterDetail> SetCurrentFilters(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove)
    {
        List<CurrentFilterDetail> currentFilters =
            !string.IsNullOrEmpty(model.SearchFilters.CurrentFiltersAppliedString) ?
                JsonSerializer.Deserialize<List<CurrentFilterDetail>>(model.SearchFilters.CurrentFiltersAppliedString) :
                    [];

        currentFilters = CheckDobFilter(model, currentFilters);

        if (model.SelectedSexValues?.Length > 0)
        {
            currentFilters.RemoveAll(currentFilterDetail =>
                currentFilterDetail.FilterType == FilterType.Sex);

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

        else if (model.SelectedSexValues == null && currentFilters.Count > 0)
        {
            currentFilters.Where((currentFilterDetail) => currentFilterDetail.FilterType == FilterType.Sex)
                .Select(currentFilterDetail => currentFilterDetail.FilterName)
                .ToList()
                .ForEach((sex) => currentFilters = RemoveFilterValue(sex, currentFilters, model));

            model.SelectedSexValues = null;
        }

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

    private static List<CurrentFilterDetail> CheckTextFilters(
        LearnerTextSearchViewModel model,
        List<CurrentFilterDetail> currentFilters,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter)
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
        if (HttpContext.Session.Keys.Contains(SortDirectionKey))
        {
            model.SortDirection = HttpContext.Session.GetString(SortDirectionKey);
        }

        if (HttpContext.Session.Keys.Contains(SortFieldKey))
        {
            model.SortField = HttpContext.Session.GetString(SortFieldKey);
        }
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

        model.LearnerNumberController = SearchLearnerNumberController;
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
    #endregion
}
