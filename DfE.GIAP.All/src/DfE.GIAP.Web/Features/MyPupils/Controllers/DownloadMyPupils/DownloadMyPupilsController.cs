using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Handlers;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MessageLevel = DfE.GIAP.Web.Features.MyPupils.Messaging.MessageLevel;

namespace DfE.GIAP.Web.Features.MyPupils.Areas.DownloadMyPupils;

[Route(Routes.MyPupilList.MyPupilsBase)]
public class DownloadMyPupilsController : Controller
{
    private readonly ILogger<DownloadMyPupilsController> _logger;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly AzureAppSettings _appSettings;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly IMyPupilsPresentationService _myPupilsPresentationService;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;

    public DownloadMyPupilsController(
        ILogger<DownloadMyPupilsController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IMyPupilsMessageSink myPupilsLogSink,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IMyPupilsPresentationService myPupilsPresentationService,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;
    }

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData(
        [FromForm] List<string> SelectedPupils,
        MyPupilsQueryRequestDto query)
            => HandleDownloadRequest(DownloadType.CTF, SelectedPupils, query);

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN([FromForm] List<string> SelectedPupils, MyPupilsQueryRequestDto query)
            => HandleDownloadRequest(DownloadType.PupilPremium, SelectedPupils, query);

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN([FromForm] List<string> SelectedPupils, MyPupilsQueryRequestDto query)
            => HandleDownloadRequest(DownloadType.NPD, SelectedPupils, query);

    [HttpPost]
    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(string selectedPupilsJoined)
    {
        string[] selectedPupils = selectedPupilsJoined.Split(',');

        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupilsJoined,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupils.Length,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile,
            SearchResultPageHeading = ApplicationLabels.SearchMyPupilListPageHeading
        };

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");

        SearchDownloadHelper.AddDownloadDataTypes(
            searchDownloadViewModel,
            User,
            User.GetOrganisationLowAge(),
            User.GetOrganisationHighAge(),
            User.IsOrganisationLocalAuthority(),
            User.IsOrganisationAllAges());

        ModelState.Clear();

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
                    return base.RedirectToAction(Routes.Application.Error, Routes.Application.Home);
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
        List<string> formSelectedPupils,
        MyPupilsQueryRequestDto query)
    {
        string userId = User.GetUserId();

        MyPupilsPupilSelectionState selectionState = _getMyPupilsStateProvider.GetPupilSelections();

        if (formSelectedPupils.Count > 0)
        {
            foreach (string selectedPupil in formSelectedPupils)
            {
                selectionState.Select(selectedPupil);
            }
            _selectionStateSessionCommandHandler.StoreInSession(selectionState);
        }

        List<string> allSelectedPupils =
            (await _myPupilsPresentationService.GetSelectedPupilsAsync(userId: User.GetUserId()))
                .ToList();

        if (allSelectedPupils.Count == 0)
        {
            _myPupilsLogSink.Add(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return RedirectToGetMyPupils(query);
        }

        if (downloadType == DownloadType.CTF && allSelectedPupils.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            _myPupilsLogSink.Add(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Downloads.Errors.UPNLimitExceeded));

            return RedirectToGetMyPupils(query);
        }

        if (downloadType == DownloadType.CTF)
        {
            ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(
                [.. allSelectedPupils],
                [.. allSelectedPupils],
                User.GetLocalAuthorityNumberForEstablishment(),
                User.GetEstablishmentNumber(),
                User.IsOrganisationEstablishment(),
                AzureFunctionHeaderDetails.Create(
                    userId,
                    User.GetSessionId()),
                ReturnRoute.MyPupilList);

            if (downloadFile.Bytes != null)
            {
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }

            _myPupilsLogSink.Add(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Downloads.Errors.NoDataForSelectedPupils));

            return RedirectToGetMyPupils(query);
        }

        if (downloadType == DownloadType.PupilPremium)
        {
            UserOrganisation userOrganisation = new()
            {
                IsAdmin = User.IsAdmin(),
                IsEstablishment = User.IsOrganisationEstablishment(),
                IsLa = User.IsOrganisationLocalAuthority(),
                IsMAT = User.IsOrganisationMultiAcademyTrust(),
                IsSAT = User.IsOrganisationSingleAcademyTrust()
            };

            ReturnFile downloadFile = await _downloadService.GetPupilPremiumCSVFile(
                allSelectedPupils.ToArray(),
                allSelectedPupils.ToArray(),
                true,
                AzureFunctionHeaderDetails.Create(
                    userId,
                    User.GetSessionId()),
                ReturnRoute.MyPupilList,
                userOrganisation);

            if (downloadFile == null)
            {
                return RedirectToAction(
                    actionName: Routes.Application.Error,
                    controllerName: Routes.Application.Home);
            }

            if (downloadFile.Bytes != null)
            {
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }

            _myPupilsLogSink.Add(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Downloads.Errors.NoDataForSelectedPupils));

            return RedirectToGetMyPupils(query);
        }

        if (downloadType == DownloadType.NPD)
        {
            return await DownloadSelectedNationalPupilDatabaseData(string.Join(",", allSelectedPupils));
        }

        _myPupilsLogSink.Add(
            new MyPupilsMessage(
                MessageLevel.Error,
                Messages.Downloads.Errors.UnknownDownloadType));

        return RedirectToGetMyPupils(query);
    }

    private RedirectToActionResult RedirectToGetMyPupils(MyPupilsQueryRequestDto request)
    {
        return RedirectToAction(
            actionName: "Index",
            controllerName: "GetMyPupils",
            new
            {
                request.PageNumber,
                request.SortField,
                request.SortDirection
            });
    }
}
