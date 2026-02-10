using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Services.Download.CTF;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Dataset = DfE.GIAP.Core.Downloads.Application.Enums.Dataset;
using MessageLevel = DfE.GIAP.Web.Features.MyPupils.Messaging.MessageLevel;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DownloadMyPupils;

[Route(Routes.MyPupilList.MyPupilsBase)]
public class DownloadMyPupilsController : Controller
{
    private readonly ILogger<DownloadMyPupilsController> _logger;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly AzureAppSettings _appSettings;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IGetSelectedPupilsUniquePupilNumbersPresentationService _getSelectedPupilsPresentationHandler;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsPupilSelectionsCommandHandler;
    private readonly IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> _downloadUseCase;
    private readonly IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> _getAvailableDatasetsForPupilsUseCase;
    private readonly IEventLogger _eventLogger;
    

    public DownloadMyPupilsController(
        ILogger<DownloadMyPupilsController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IMyPupilsMessageSink myPupilsLogSink,
        IDownloadCommonTransferFileService ctfService,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectedPupilsPresentationHandler,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsPupilSelectionsCommandHandler,
        IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse> downloadUseCase,
        IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> getAvailableDatasetsForPupilsUseCase,
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

        ArgumentNullException.ThrowIfNull(getAvailableDatasetsForPupilsUseCase);
        _getAvailableDatasetsForPupilsUseCase = getAvailableDatasetsForPupilsUseCase;
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
        if (string.IsNullOrEmpty(model.SelectedPupils))
        {
            model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
            TempData["ErrorDetails"] = model.ErrorDetails;
            return await GetDownloadNpdOptions(model.SelectedPupils);
        }

        string[] selectedPupils = model.SelectedPupils.Split(',');

        if (model.SelectedDownloadOptions == null)
        {
            model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            TempData["ErrorDetails"] = model.ErrorDetails;
            return await GetDownloadNpdOptions(model.SelectedPupils);
        }

        if (model.DownloadFileType != DownloadFileType.None)
        {
            // Note: Applied from Search impl
            List<Dataset> selectedDatasets =
                model.SelectedDownloadOptions
                    .Where(datasetString => Enum.TryParse(datasetString, ignoreCase: true, out Dataset _))
                    .Select(datasetString => Enum.Parse<Dataset>(datasetString, ignoreCase: true))
                    .ToList();

            DownloadPupilDataRequest request = new(
               SelectedPupils: selectedPupils,
               SelectedDatasets: selectedDatasets,
               DownloadType: Core.Downloads.Application.Enums.DownloadType.NPD,
               FileFormat: model.DownloadFileType == DownloadFileType.CSV ? FileFormat.Csv : FileFormat.Tab);

            DownloadPupilDataResponse response = await _downloadUseCase.HandleRequestAsync(request);

            string loggingBatchId = Guid.NewGuid().ToString();

            IEnumerable<Core.Common.CrossCutting.Logging.Events.Dataset> datasetsToLog = model.SelectedDownloadOptions
                .Select(dataset => new
                {
                    Success = Enum.TryParse(dataset, out Core.Common.CrossCutting.Logging.Events.Dataset Parsed),
                    Parsed
                })
                .Where(x => x.Success)
                .Select(x => x.Parsed);

            foreach (Core.Common.CrossCutting.Logging.Events.Dataset datasetEnum in datasetsToLog)
            {
                _eventLogger.LogDownload(
                    Core.Common.CrossCutting.Logging.Events.DownloadType.MyPupils,
                    model.DownloadFileType == DownloadFileType.CSV ? DownloadFileFormat.CSV : DownloadFileFormat.TAB,
                    DownloadEventType.NPD,
                    loggingBatchId,
                    datasetEnum);
            }
            // End: applied from Search

            if (response is null)
            {
                return RedirectToAction(Routes.Application.Error, Routes.Application.Home);
            }

            if (response.FileContents is not null)
            {
                model.ErrorDetails = null;
                return File(response.FileContents, response.ContentType, response.FileName);
            }
            else
            {
                model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
                TempData["ErrorDetails"] = model.ErrorDetails;
                return await GetDownloadNpdOptions(model.SelectedPupils);
            }
        }
        
        model.ErrorDetails = Messages.Search.Errors.SelectFileType;
        TempData["ErrorDetails"] = model.ErrorDetails;
        return await GetDownloadNpdOptions(model.SelectedPupils);
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


        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");

        ModelState.Clear();

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

        return View(Global.MPLDownloadNPDOptionsView, searchDownloadViewModel);
    }


    private async Task<List<string>> UpsertSelectedPupilsAsync(MyPupilsFormStateRequestDto updateForm)
    {
        if (updateForm != null)
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
