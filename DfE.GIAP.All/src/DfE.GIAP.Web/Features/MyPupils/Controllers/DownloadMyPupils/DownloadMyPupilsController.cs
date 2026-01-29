using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
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

    public DownloadMyPupilsController(
        ILogger<DownloadMyPupilsController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IMyPupilsMessageSink myPupilsLogSink,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectedPupilsPresentationHandler,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsPupilSelectionsCommandHandler)
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
    }

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToDownloadCommonTransferFileData(
        [FromForm] List<string> SelectedPupils,
        MyPupilsQueryRequestDto query)
    {
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(SelectedPupils);

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
        [FromForm] List<string> SelectedPupils, MyPupilsQueryRequestDto query, CancellationToken ctx = default)
    {
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(SelectedPupils);

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
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToDownloadSelectedNPDDataUPN([FromForm] List<string> SelectedPupils, MyPupilsQueryRequestDto query)
    {
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(SelectedPupils);

        if (updatedPupils.Count == 0)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return RedirectToGetMyPupils(query);
        }

        return await DownloadSelectedNationalPupilDatabaseData(string.Join(",", updatedPupils));
    }

    [HttpPost]
    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [ValidateAntiForgeryToken]
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

    private async Task<List<string>> UpsertSelectedPupilsAsync(List<string> selectedPupils)
    {
        MyPupilsFormStateRequestDto request = new()
        {
            SelectedPupils = selectedPupils
        };

        await _updateMyPupilsPupilSelectionsCommandHandler.Handle(request);

        List<string> allSelectedPupils =
            (await _getSelectedPupilsPresentationHandler.GetSelectedPupilsAsync(userId: User.GetUserId()))
                .ToList();

        return allSelectedPupils;
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
