using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;
using DfE.GIAP.Web.Features.MyPupils.ViewModels;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

[Route(Routes.Application.MyPupilList)]
public class MyPupilListController : Controller
{
    private readonly ILogger<MyPupilListController> _logger;
    private readonly AzureAppSettings _appSettings;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deleteMyPupilsUseCase;
    private readonly IGetMyPupilsForUserHandler _getMyPupilsForUserHandler;
    private readonly IUpdateMyPupilsStateHandler _updateMyPupilsStateHandler;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly IGetSelectedMyPupilsProvider _getSelectedMyPupilsProvider;

    public MyPupilListController(
        ILogger<MyPupilListController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deleteMyPupilsUseCase,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        IGetMyPupilsForUserHandler getMyPupilsForUserHandler,
        IUpdateMyPupilsStateHandler updateMyPupilsStateHandler,
        IGetSelectedMyPupilsProvider getSelectedMyPupilsProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(deleteMyPupilsUseCase);
        _deleteMyPupilsUseCase = deleteMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(getMyPupilsForUserHandler);
        _getMyPupilsForUserHandler = getMyPupilsForUserHandler;

        ArgumentNullException.ThrowIfNull(updateMyPupilsStateHandler);
        _updateMyPupilsStateHandler = updateMyPupilsStateHandler;

        ArgumentNullException.ThrowIfNull(getSelectedMyPupilsProvider);
        _getSelectedMyPupilsProvider = getSelectedMyPupilsProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(MyPupilsErrorViewModel? error = null)
    {
        _logger.LogInformation("My pupil list GET method is called");

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        GetMyPupilsForUserRequest request = new(
            UserId: new UserId(User.GetUserId()),
            state);

        PupilsViewModel pupilsResponse = await _getMyPupilsForUserHandler.HandleAsync(request);

        MyPupilsViewModel viewModel = new(
            pupils: pupilsResponse,
            error: !string.IsNullOrEmpty(error?.Message) ? error : null)
        {
            PageNumber = state.PresentationState.Page,
            SortDirection = state.PresentationState.SortDirection == SortDirection.Ascending ? "asc" : "desc",
            SortField = state.PresentationState.SortBy,
            IsAnyPupilsSelected = state.SelectionState.IsAnyPupilSelected,
            SelectAll = state.SelectionState.IsAllPupilsSelected,
        };

        return View(Routes.MyPupilList.MyPupilListView, viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> MyPupilList(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("My pupil list POST method called");

        if (formDto.Error is not null)
        {
            return await Index(formDto.Error);
        }

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            return await Index(error);
        }

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        await _updateMyPupilsStateHandler.HandleAsync(
            new UpdateMyPupilsStateRequest(
                UserId: new UserId(User.GetUserId()),
                State: state,
                UpdateStateInput: formDto));

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route(Routes.MyPupilList.RemoveSelected)]
    public async Task<IActionResult> RemoveSelected(
        [FromForm] bool? SelectAll,
        [FromForm] List<string> SelectedPupils)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            return await Index(error);
        }

        if (SelectedPupils.Count == 0 && !SelectAll.HasValue)
        {
            MyPupilsErrorViewModel myPupilsErrorModel = new(Messages.Common.Errors.NoPupilsSelected);
            return await Index(myPupilsErrorModel);
        }

        bool deleteAllPupils = SelectAll.HasValue && SelectAll.Value;

        DeletePupilsFromMyPupilsRequest deleteRequest = new(
                UserId: new UserId(User.GetUserId()),
                DeletePupilUpns: SelectedPupils.ToUniquePupilNumbers(),
                DeleteAll: deleteAllPupils);

        await _deleteMyPupilsUseCase.HandleRequestAsync(deleteRequest);

        return RedirectToAction(nameof(Index));
    }

    // Downloads

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData([FromForm] List<string>? SelectedPupils) => HandleDownloadRequest(DownloadType.CTF, SelectedPupils); // TODO reduce what's posted to necessary

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN([FromForm] List<string>? SelectedPupils) => HandleDownloadRequest(DownloadType.PupilPremium, SelectedPupils);

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN([FromForm] List<string>? SelectedPupils) => HandleDownloadRequest(DownloadType.NPD, SelectedPupils);

    [HttpPost]
    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
        string selectedPupilsJoined)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupilsJoined,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsJoined.Length,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        SearchDownloadHelper.AddDownloadDataTypes(
            searchDownloadViewModel,
            User,
            User.GetOrganisationLowAge(),
            User.GetOrganisationHighAge(),
            User.IsOrganisationLocalAuthority(),
            User.IsOrganisationAllAges());

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        ModelState.Clear();
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = "MyPupilList";
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;

        string[] selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            string[] downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            IEnumerable<CheckDownloadDataType> disabledTypes = await _downloadService.CheckForNoDataAvailable(
                selectedPupils,
                selectedPupils,
                downloadTypeArray,
                AzureFunctionHeaderDetails.Create(
                    User.GetUserId(),
                    User.GetSessionId()));

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
            string[] selectedPupils = model.SelectedPupils.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                ReturnFile downloadFile = model.DownloadFileType == DownloadFileType.CSV
                    ? await _downloadService.GetCSVFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase)
                    : await _downloadService.GetTABFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase);

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
            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }


    private async Task<IActionResult> HandleDownloadRequest(
        DownloadType downloadType,
        List<string> selectedPupils)
    {
        List<UniquePupilNumber> selectedPupilsFromForm = selectedPupils?.ToUniquePupilNumbers().ToList() ?? [];
        UniquePupilNumbers selectedPupilsFromState = _getSelectedMyPupilsProvider.GetSelectedMyPupils();

        UniquePupilNumbers joinedSelectedPupils =
            UniquePupilNumbers.Create(
                uniquePupilNumbers:
                    selectedPupilsFromForm.Concat(selectedPupilsFromState.GetUniquePupilNumbers()).Distinct());

        if (joinedSelectedPupils.IsEmpty)
        {
            MyPupilsErrorViewModel error = new(Messages.Common.Errors.NoPupilsSelected);
            return await Index(error);
        }

        // TODO do I need to UpdateSelectedPupilsHere in state, as the user may have selected pupils on page, and then want to download other data?

        return downloadType switch
        {
            DownloadType.CTF => await DownloadCommonTransferFileData(joinedSelectedPupils),
            DownloadType.PupilPremium => await DownloadPupilPremiumData(joinedSelectedPupils),
            DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(string.Join(",", joinedSelectedPupils.GetUniquePupilNumbers().Select(t => t.Value))),
            _ => await Index(new MyPupilsErrorViewModel(Messages.Downloads.Errors.UnknownDownloadType))
        };
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(UniquePupilNumbers selectedPupils)
    {
        string[] selectedPupilsInput = selectedPupils.GetUniquePupilNumbers().Select(t => t.Value).ToArray();

        if (selectedPupils.Count > _appSettings.CommonTransferFileUPNLimit) // TODO check this works pulled from HandleDownloadReq
        {
            MyPupilsErrorViewModel error = new(Messages.Downloads.Errors.UPNLimitExceeded);
            return View(Routes.MyPupilList.MyPupilListView, error);
        }

        ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(
            selectedPupilsInput,
            selectedPupilsInput,
            User.GetLocalAuthorityNumberForEstablishment(),
            User.GetEstablishmentNumber(),
            User.IsOrganisationEstablishment(),
            AzureFunctionHeaderDetails.Create(
                User.GetUserId(),
                User.GetSessionId()),
            ReturnRoute.MyPupilList);

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        return await Index(new MyPupilsErrorViewModel(Messages.Downloads.Errors.NoDataForSelectedPupils));
    }

    private async Task<IActionResult> DownloadPupilPremiumData(UniquePupilNumbers selectedPupils)
    {
        UserOrganisation userOrganisation = new()
        {
            IsAdmin = User.IsAdmin(),
            IsEstablishment = User.IsOrganisationEstablishment(),
            IsLa = User.IsOrganisationLocalAuthority(),
            IsMAT = User.IsOrganisationMultiAcademyTrust(),
            IsSAT = User.IsOrganisationSingleAcademyTrust()
        };

        string[] selectedPupilInput = selectedPupils.GetUniquePupilNumbers().Select(t => t.Value).ToArray();

        ReturnFile downloadFile = await _downloadService.GetPupilPremiumCSVFile(
            selectedPupilInput,
            selectedPupilInput,
            true,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            ReturnRoute.MyPupilList, userOrganisation);

        if (downloadFile == null)
        {
            return RedirectToAction(actionName: Routes.Application.Error, controllerName: Routes.Application.Home);
        }

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        return await Index(new MyPupilsErrorViewModel(Messages.Downloads.Errors.NoDataForSelectedPupils));
    }
}
