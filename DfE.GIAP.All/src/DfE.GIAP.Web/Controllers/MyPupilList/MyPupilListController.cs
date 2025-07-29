using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

[Route(Routes.Application.MyPupilList)]
public class MyPupilListController : Controller
{
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";

    private readonly ILogger<MyPupilListController> _logger;
    private readonly ICommonService _commonService;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsFromMyPupilsuseCase;
    private readonly IPaginatedSearchService _paginatedSearch;
    private readonly ISelectionManager _selectionManager;
    private readonly IMyPupilListService _mplService;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    public MyPupilListController(
        ILogger<MyPupilListController> logger,
        IPaginatedSearchService paginatedSearch,
        IMyPupilListService mplService,
        ISelectionManager selectionManager,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        ICommonService commonService,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsFromMyPupilsuseCase)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(commonService);
        _commonService = commonService;

        ArgumentNullException.ThrowIfNull(getMyPupilsUseCase);
        _getMyPupilsUseCase = getMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(deletePupilsFromMyPupilsuseCase);
        _deletePupilsFromMyPupilsuseCase = deletePupilsFromMyPupilsuseCase;

        ArgumentNullException.ThrowIfNull(paginatedSearch);
        _paginatedSearch = paginatedSearch;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(mplService);
        _mplService = mplService;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;
    }


    [HttpGet]
    public async Task<IActionResult> Index() // TODO call to MyPupilList with Page=0, SortField = "", SortDir = "" (Add Default to MyPupilOptions
    {
        _logger.LogInformation("My pupil list GET method is called");

        GetMyPupilsRequest request = new(UserId: User.GetUserId());

        GetMyPupilsResponse getMyPupilsResponse = await _getMyPupilsUseCase.HandleRequestAsync(request);

        IEnumerable<PupilPresentatationModel> pupils = getMyPupilsResponse.Pupils.Select((t) => new PupilPresentatationModel(t));

        MyPupilsViewModel model = new(pupils);

        return View(Routes.MyPupilList.MyPupilListView, model);
    }

    [HttpPost]
    public async Task<IActionResult> MyPupilList(
        MyPupilListViewModel model, // TODO take in PaginatedMyPupilsRequestDt and hydrate MyPupilsViewModel. WAITING on removal of calls from Remove / Download to this endpoint
        [FromQuery] int pageNumber)
    //,
    //bool calledByController = false,
    //bool failedDownload = false)
    {
        _logger.LogInformation("My pupil list UPN POST method called");

        if (!ModelState.IsValid)
        {
            MyPupilsErrorModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            MyPupilsViewModel outputModelOnError = new([], error); // Should this attempt to use existing pupils if they exist or just return empty?
            return View(Routes.MyPupilList.MyPupilListView, outputModelOnError);
        }

        OrderPupilsBy order = new(
            field: model.SortField,
            direction: model.SortDirection switch
            {
                "asc" => SortDirection.Ascending,
                "desc" => SortDirection.Descending,
                _ => SortDirection.Default
            });

        GetMyPupilsRequest request = new(
            UserId: User.GetUserId(),
            Options: new MyPupilsQueryOptions(
                order,
                PageNumber.Page(pageNumber)));

        GetMyPupilsResponse getMyPupilsResponse = await _getMyPupilsUseCase.HandleRequestAsync(request);

        IEnumerable<PupilPresentatationModel> pupils = getMyPupilsResponse.Pupils.Select((t) => new PupilPresentatationModel(t));

        MyPupilsViewModel presentationModel = new(pupils)
        {
            PageNumber = pageNumber,
            SortField = model.SortField,
            SortDirection = model.SortDirection,
            //SelectedPupils = model.SelectedPupil
        };

        return View(Routes.MyPupilList.MyPupilListView, presentationModel);
    }

    [HttpPost]
    [Route(Routes.MyPupilList.RemoveSelected)]
    public async Task<IActionResult> RemoveSelected(
        // TODO flag for ApplyToAllPupils
        // TODO also post the PaginatedOptions so the removal can happen, and the user is kept on the same reloaded page, MAY need to page back then if they remove everything on current page?
        //[FromQuery] int pageNumber,
        bool SelectAllNoJsChecked,
        List<string> SelectedPupil)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: User.GetUserId(),
            SelectedPupil,
            DeleteAll: SelectAllNoJsChecked);

        await _deletePupilsFromMyPupilsuseCase.HandleRequestAsync(request);
        return RedirectToAction(nameof(Index));

    }

    public sealed class MyPupilsFormStateRequestDto
    {
        public string SortField { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public bool SelectAllNoJsChecked { get; set; }
        public IEnumerable<string> SelectedPupil { get; set; } = [];
    }



    /***** OLD IMPLEMENTATION BELOW */
    // There's a select all flag which is passed, not sure what relevance that has for sorting / paging though, only relevant for Removal / Downloads.

    //var allSelected = false;

    //model.Upn = SecurityHelper.SanitizeText(model.Upn);

    //if (ModelState.IsValid)
    //{
    //    if (!string.IsNullOrEmpty(model.SelectAllNoJsChecked))
    //    {
    //        var selectAll = Convert.ToBoolean(model.SelectAllNoJsChecked);
    //        var upns = model.Upn.FormatLearnerNumbers();
    //        if (selectAll)
    //        {
    //            _selectionManager.AddAll(upns);
    //            model.ToggleSelectAll = true;
    //        }
    //        else
    //        {
    //            _selectionManager.RemoveAll(upns);
    //            model.ToggleSelectAll = false;
    //        }

    //        model.SelectAllNoJsChecked = null;
    //        allSelected = true;
    //    }

    //    if (!notPaged && !allSelected && !failedDownload)
    //    {
    //        SetSelections(model.PageLearnerNumbers.Split(','), model.SelectedPupil);
    //    }

    //    model = await GetPupilsForSearchBuilder(model, pageNumber, notPaged);
    //    model.PageNumber = pageNumber;
    //    model.PageSize = PAGESIZE;
    //}

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData(MyPupilListViewModel model)
        => HandleDownloadRequest(model, DownloadType.CTF);

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(MyPupilListViewModel model)
        => HandleDownloadRequest(model, DownloadType.PupilPremium);

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN(MyPupilListViewModel model)
        => HandleDownloadRequest(model, DownloadType.NPD);

    private async Task<IActionResult> HandleDownloadRequest(MyPupilListViewModel model, DownloadType downloadType)
    {
        SetSelections(model.PageLearnerNumbers.Split(','), model.SelectedPupil);
        var selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());

        if (selectedPupils == null || selectedPupils.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber);
        }

        if (downloadType == DownloadType.CTF && selectedPupils.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            model.ErrorDetails = Messages.Downloads.Errors.UPNLimitExceeded;
            return await MyPupilList(model, model.PageNumber);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupils.ToArray()) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            PrepareStarredPupilConfirmation(model, selectedPupils, downloadType);
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        return downloadType switch
        {
            DownloadType.CTF => await DownloadCommonTransferFileData(model, selectedPupils.ToArray()),
            DownloadType.PupilPremium => await DownloadPupilPremiumData(model, selectedPupils.ToArray()),
            DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(string.Join(',', selectedPupils), model.Upn, selectedPupils.Count),
            _ => await MyPupilList(model, model.PageNumber)
        };
    }

    private void PrepareStarredPupilConfirmation(MyPupilListViewModel model, HashSet<string> selectedPupils, DownloadType downloadType)
    {
        var confirmationModel = model.StarredPupilConfirmationViewModel;
        confirmationModel.SelectedPupil = string.Join(',', selectedPupils);
        PopulateConfirmationViewModel(confirmationModel, model);
        confirmationModel.DownloadType = downloadType;
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(MyPupilListViewModel model, string[] selectedPupils)
    {
        var downloadFile = await _ctfService.GetCommonTransferFile(
            selectedPupils,
            model.Upn.FormatLearnerNumbers(),
            User.GetLocalAuthorityNumberForEstablishment(),
            User.GetEstablishmentNumber(),
            User.IsOrganisationEstablishment(),
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            ReturnRoute.MyPupilList);

        if (downloadFile.Bytes != null)
            return SearchDownloadHelper.DownloadFile(downloadFile);

        model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
        return await MyPupilList(model, model.PageNumber);
    }

    private async Task<IActionResult> DownloadPupilPremiumData(MyPupilListViewModel model, string[] selectedPupils)
    {
        var userOrganisation = new UserOrganisation
        {
            IsAdmin = User.IsAdmin(),
            IsEstablishment = User.IsOrganisationEstablishment(),
            IsLa = User.IsOrganisationLocalAuthority(),
            IsMAT = User.IsOrganisationMultiAcademyTrust(),
            IsSAT = User.IsOrganisationSingleAcademyTrust()
        };

        var downloadFile = await _downloadService.GetPupilPremiumCSVFile(
            selectedPupils, selectedPupils, true,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            ReturnRoute.MyPupilList, userOrganisation);

        if (downloadFile == null)
            return RedirectToAction(Routes.Application.Error, Routes.Application.Home);

        if (downloadFile.Bytes != null)
            return SearchDownloadHelper.DownloadFile(downloadFile);

        model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
        return await MyPupilList(model, model.PageNumber);
    }

    [HttpPost]
    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
        string selectedPupilsJoined,
        string learnerNumber,
        int selectedPupilsCount)
    {
        var searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupilsJoined,
            LearnerNumber = learnerNumber,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        SearchDownloadHelper.AddDownloadDataTypes(
            searchDownloadViewModel, User, User.GetOrganisationLowAge(),
            User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(),
            User.IsOrganisationAllAges());

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        ModelState.Clear();
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = "MyPupilList";
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;

        var selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForNoDataAvailable(
                selectedPupils, selectedPupils, downloadTypeArray,
                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));
            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }

        searchDownloadViewModel.SearchResultPageHeading = ApplicationLabels.SearchMyPupilListPageHeading;
        return View(Global.MPLDownloadNPDOptionsView, searchDownloadViewModel);
    }

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedPupils))
        {
            var selectedPupils = model.SelectedPupils.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = model.DownloadFileType == DownloadFileType.CSV
                    ? await _downloadService.GetCSVFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase)
                    : await _downloadService.GetTABFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase);

                if (downloadFile == null)
                    return RedirectToAction(Routes.Application.Error, Routes.Application.Home);

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
            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }

    [HttpPost]
    [Route(Routes.MyPupilList.DownloadNonUPNConfirmationReturn)]
    public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationError = !model.ConfirmationGiven;
        PopulateConfirmationViewModel(model);

        if (model.ConfirmationGiven)
        {
            var joinedLearnerNumbers = model.SelectedPupil.Split(",");
            if (PupilHelper.CheckIfStarredPupil(model.SelectedPupil))
            {
                model.SelectedPupil = string.Join(",", RbacHelper.DecryptUpnCollection(joinedLearnerNumbers));
            }

            await _mplService.UpdatePupilMasks(model.SelectedPupil.Split(",").ToList(), false, User.GetUserId(), AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

            return model.DownloadType switch
            {
                DownloadType.CTF => await DownloadCommonTransferFileData(new MyPupilListViewModel { SelectedPupil = model.SelectedPupil.Split(",").ToList(), Upn = model.LearnerNumbers }, model.SelectedPupil.Split(",")),
                DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupil, model.LearnerNumbers, joinedLearnerNumbers.Length),
                DownloadType.PupilPremium => await DownloadPupilPremiumData(new MyPupilListViewModel { SelectedPupil = model.SelectedPupil.Split(",").ToList(), Upn = model.LearnerNumbers }, model.SelectedPupil.Split(",")),
                _ => ConfirmationForStarredPupil(model)
            };
        }

        return ConfirmationForStarredPupil(model);
    }

    [NonAction]
    public IActionResult ConfirmationForStarredPupil(StarredPupilConfirmationViewModel model)
        => View(Global.StarredPupilConfirmationView, model);


    private HashSet<string> GetSelected(string[] available)
    {
        var missing = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));
        var actuallyAvailable = missing != null ? available.Except(missing).ToArray() : available;
        return _selectionManager.GetSelected(actuallyAvailable);
    }

    private void SetSelections(IEnumerable<string> available, IEnumerable<string> selected)
    {
        var toAdd = selected ?? Enumerable.Empty<string>();
        var toRemove = available.Except(toAdd);
        _selectionManager.AddAll(toAdd);
        _selectionManager.RemoveAll(toRemove);
    }

    private void PopulateConfirmationViewModel(StarredPupilConfirmationViewModel model, MyPupilListViewModel mplModel = null)
    {
        model.ConfirmationReturnController = Global.MyPupilListControllerName;
        model.ConfirmationReturnAction = Global.MyPupilListDownloadConfirmationReturnAction;
        model.CancelReturnController = Global.MyPupilListControllerName;
        model.CancelReturnAction = Global.MyPupilListDownloadCancellationReturnAction;
        if (mplModel != null)
            model.LearnerNumbers = mplModel.Upn;
    }
}



