using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Services.Download.CTF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DownloadMyPupils;

[Route(Routes.MyPupilList.MyPupilsBase)]
public class DownloadMyPupilsCommonTransferFileController : Controller
{
    private readonly ILogger<DownloadMyPupilsNationalPupilDatabaseController> _logger;
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IGetSelectedPupilsUniquePupilNumbersPresentationService _getSelectedPupilsPresentationHandler;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsPupilSelectionsCommandHandler;

    public DownloadMyPupilsCommonTransferFileController(ILogger<DownloadMyPupilsNationalPupilDatabaseController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IMyPupilsMessageSink myPupilsLogSink,
        IDownloadCommonTransferFileService ctfService,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectedPupilsPresentationHandler,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsPupilSelectionsCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _azureAppSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(getSelectedPupilsPresentationHandler);
        _getSelectedPupilsPresentationHandler = getSelectedPupilsPresentationHandler;

        ArgumentNullException.ThrowIfNull(updateMyPupilsPupilSelectionsCommandHandler);
        _updateMyPupilsPupilSelectionsCommandHandler = updateMyPupilsPupilSelectionsCommandHandler;
    }
    
    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        MyPupilsFormStateRequestDto updateFormRequest,
        MyPupilsQueryRequestDto? query)
    {
        query ??= new();

        List<string> updatedPupils = await UpsertSelectedPupilsAsync(updateFormRequest);

        if (updatedPupils.Count == 0)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return MyPupilsRedirectHelpers.RedirectToGetMyPupils(query);
        }

        if (updatedPupils.Count > _azureAppSettings.CommonTransferFileUPNLimit)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    MessageLevel.Error,
                    Messages.Downloads.Errors.UPNLimitExceeded));

            return MyPupilsRedirectHelpers.RedirectToGetMyPupils(query);
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

        return MyPupilsRedirectHelpers.RedirectToGetMyPupils(query);
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
}
