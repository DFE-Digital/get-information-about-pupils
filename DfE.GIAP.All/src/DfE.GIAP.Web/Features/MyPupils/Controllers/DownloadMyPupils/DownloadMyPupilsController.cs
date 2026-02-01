using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilIdentifiers;
using DfE.GIAP.Web.Helpers.SearchDownload;
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
    private readonly IGetSelectedPupilsUniquePupilNumbersPresentationService _getSelectedPupilsPresentationHandler;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsPupilSelectionsCommandHandler;
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadUseCase;
    private readonly IEventLogger _eventLogger;

    public DownloadMyPupilsController(
        ILogger<DownloadMyPupilsController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IMyPupilsMessageSink myPupilsLogSink,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectedPupilsPresentationHandler,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsPupilSelectionsCommandHandler,
        IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> downloadUseCase,
        IEventLogger eventLogger)
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

        ArgumentNullException.ThrowIfNull(getSelectedPupilsPresentationHandler);
        _getSelectedPupilsPresentationHandler = getSelectedPupilsPresentationHandler;

        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupilsService);
        _downloadPupilPremiumDataForPupilsService = downloadPupilPremiumDataForPupilsService;

        ArgumentNullException.ThrowIfNull(updateMyPupilsPupilSelectionsCommandHandler);
        _updateMyPupilsPupilSelectionsCommandHandler = updateMyPupilsPupilSelectionsCommandHandler;

        ArgumentNullException.ThrowIfNull(downloadUseCase);
        _downloadUseCase = downloadUseCase;

        ArgumentNullException.ThrowIfNull(eventLogger);
        _eventLogger = eventLogger;
    }

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToDownloadCommonTransferFileData(
        MyPupilsFormStateRequestDto updateForm,
        MyPupilsQueryRequestDto query)
    {
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(updateForm);

        if (updatedPupils.Count == 0)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return RedirectToGetMyPupils(query);
        }

        if (updatedPupils.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            _myPupilsLogSink.AddMessage(
            new MyPupilsMessage(
                MessageLevel.Error,
                Messages.Downloads.Errors.UPNLimitExceeded));

            return RedirectToGetMyPupils(query);
        }

        ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(
            [.. updatedPupils],
            [.. updatedPupils],
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

        _myPupilsLogSink.AddMessage(
            new MyPupilsMessage(
                MessageLevel.Error,
                Messages.Downloads.Errors.NoDataForSelectedPupils));

        return RedirectToGetMyPupils(query);
    }


    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(
        MyPupilsFormStateRequestDto updateForm,
        MyPupilsQueryRequestDto query,
        CancellationToken ctx = default)
    {
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(updateForm);

        if (updatedPupils.Count == 0)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return RedirectToGetMyPupils(query);
        }

        DownloadPupilPremiumFilesResponse response = await
            _downloadPupilPremiumDataForPupilsService.DownloadAsync(
                updatedPupils,
                Core.Common.CrossCutting.Logging.Events.DownloadType.MyPupils,
                ctx);

        if (response is null)
        {
            return RedirectToAction(
                actionName: Routes.Application.Error,
                controllerName: Routes.Application.Home);
        }

        if (!response.HasData)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Downloads.Errors.NoDataForSelectedPupils));

            return RedirectToGetMyPupils(query);
        }

        return response.GetResult();
    }

    [HttpPost]
    [Route(Routes.MyPupilList.DownloadOptionsRoute)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetDownloadNpdOptions(MyPupilsFormStateRequestDto updateForm, MyPupilsQueryRequestDto query)
    {
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(updateForm);

        if (updatedPupils.Count == 0)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return RedirectToGetMyPupils(query);
        }

        return await GetDownloadNpdOptions(string.Join(",", updatedPupils));
    }


    [HttpPost]
    [Route(Routes.MyPupilList.DownloadConfirmRoute)]
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
                // Note: Applied from Search impl
                List<Core.Downloads.Application.Enums.Dataset> selectedDatasets = new();

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

                DownloadPupilDataResponse response = await _downloadUseCase.HandleRequestAsync(request);

                string loggingBatchId = Guid.NewGuid().ToString();

                foreach (string dataset in model.SelectedDownloadOptions)
                {
                    // TODO: Temp quick solution
                    if (Enum.TryParse(dataset, out Core.Common.CrossCutting.Logging.Events.Dataset datasetEnum))
                    {
                        _eventLogger.LogDownload(
                            Core.Common.CrossCutting.Logging.Events.DownloadType.MyPupils,
                            model.DownloadFileType == DownloadFileType.CSV ? DownloadFileFormat.CSV : DownloadFileFormat.TAB,
                            DownloadEventType.NPD,
                            loggingBatchId,
                            datasetEnum);
                    }
                }
                // End: applied from Search

                if (response is null)
                {
                    return base.RedirectToAction(Routes.Application.Error, Routes.Application.Home);
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
            return await GetDownloadNpdOptions(model.SelectedPupils);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }

    private async Task<IActionResult> GetDownloadNpdOptions(string selectedPupilsJoined)
    {
        string[] selectedPupils = selectedPupilsJoined.Split(',');

        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            LearnerNumber = selectedPupilsJoined,
            SelectedPupils = selectedPupilsJoined,
            ErrorDetails = (string)(TempData["ErrorDetails"] ?? string.Empty),
            SelectedPupilsCount = selectedPupils.Length,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            DownloadRoute = Routes.MyPupilList.DownloadConfirmRoute,
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


    private async Task<List<string>> UpsertSelectedPupilsAsync(MyPupilsFormStateRequestDto? updateForm)
    {
        if(updateForm != null)
        {
            await _updateMyPupilsPupilSelectionsCommandHandler.Handle(updateForm);
        }

        List<string> allSelectedPupils =
            (await _getSelectedPupilsPresentationHandler.GetSelectedPupilsAsync(userId: User.GetUserId()))
                .ToList();

        return allSelectedPupils;
    }

    private RedirectToActionResult RedirectToGetMyPupils(MyPupilsQueryRequestDto request)
    {
        return RedirectToAction(
            actionName: "Index",
            controllerName: Routes.MyPupilList.GetMyPupilsController,
            new
            {
                request.PageNumber,
                request.SortField,
                request.SortDirection
            });
    }
}
