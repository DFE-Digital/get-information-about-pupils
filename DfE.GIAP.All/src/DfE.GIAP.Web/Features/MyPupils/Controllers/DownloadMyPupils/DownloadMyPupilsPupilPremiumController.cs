using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Services.UpsertSelectedPupils;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DownloadMyPupils;
[Route(Routes.MyPupilList.MyPupilsBase)]
public class DownloadMyPupilsPupilPremiumController : Controller
{
    private readonly ILogger<DownloadMyPupilsNationalPupilDatabaseController> _logger;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IUpsertSelectedPupilsIdentifiersPresentationService _upsertSelectedPupilsPresentationService;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;

    public DownloadMyPupilsPupilPremiumController(
        ILogger<DownloadMyPupilsNationalPupilDatabaseController> logger,
        IMyPupilsMessageSink myPupilsLogSink,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService,
        IUpsertSelectedPupilsIdentifiersPresentationService upsertSelectedPupilsPresentationService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        
        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupilsService);
        _downloadPupilPremiumDataForPupilsService = downloadPupilPremiumDataForPupilsService;

        ArgumentNullException.ThrowIfNull(upsertSelectedPupilsPresentationService);
        _upsertSelectedPupilsPresentationService = upsertSelectedPupilsPresentationService;
    }

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        MyPupilsPupilSelectionsRequestDto? selectionsDto,
        MyPupilsQueryRequestDto? query,
        CancellationToken ctx = default)
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

        DownloadPupilPremiumFilesResponse response = await
            _downloadPupilPremiumDataForPupilsService.DownloadAsync(
                updatedPupils,
                DownloadType.MyPupils,
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

            return MyPupilsRedirectHelpers.RedirectToGetMyPupils(query);
        }

        return response.GetResult();
    }
}
