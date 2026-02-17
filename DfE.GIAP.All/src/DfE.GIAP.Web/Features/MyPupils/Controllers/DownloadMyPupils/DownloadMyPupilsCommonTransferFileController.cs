using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;
using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Services.UpsertSelectedPupils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DownloadMyPupils;

[Route(Routes.MyPupilList.MyPupilsBase)]
public class DownloadMyPupilsCommonTransferFileController : Controller
{
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IUpsertSelectedPupilsIdentifiersPresentationService _upsertSelectedPupilsPresentationService;
    private readonly IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse> _downloadPupilCtfUseCase;
    private readonly IEventLogger _eventLogger;

    public DownloadMyPupilsCommonTransferFileController(
        IOptions<AzureAppSettings> azureAppSettings,
        IMyPupilsMessageSink myPupilsLogSink,
        IUpsertSelectedPupilsIdentifiersPresentationService upsertSelectedPupilsPresentationService,
        IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse> downloadPupilCtfUseCase,
        IEventLogger eventLogger)
    {

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _azureAppSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(upsertSelectedPupilsPresentationService);
        _upsertSelectedPupilsPresentationService = upsertSelectedPupilsPresentationService;

        ArgumentNullException.ThrowIfNull(downloadPupilCtfUseCase);
        _downloadPupilCtfUseCase = downloadPupilCtfUseCase;

        ArgumentNullException.ThrowIfNull(eventLogger);
        _eventLogger = eventLogger;
    }

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        MyPupilsPupilSelectionsRequestDto selectionsDto,
        MyPupilsQueryRequestDto? query)
    {
        query ??= new();

        List<string> updatedPupils =
            await _upsertSelectedPupilsPresentationService.UpsertAsync(
                userId: User.GetUserId(),
                selectionsDto);

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

        DownloadPupilCtfRequest request = new(
            SelectedPupils: [.. updatedPupils],
            IsEstablishment: User.IsOrganisationEstablishment(),
            LocalAuthoriyNumber: User.GetLocalAuthorityNumberForEstablishment(),
            EstablishmentNumber: User.GetEstablishmentNumber());

        DownloadPupilCtfResponse downloadPupilCtfResponse = await _downloadPupilCtfUseCase.HandleRequestAsync(request);

        _eventLogger.LogDownload(DownloadType.Search, DownloadFileFormat.XML, DownloadEventType.CTF);

        return File(
            fileStream: downloadPupilCtfResponse.FileStream,
            contentType: downloadPupilCtfResponse.ContentType,
            fileDownloadName: downloadPupilCtfResponse.FileName);
    }
}
