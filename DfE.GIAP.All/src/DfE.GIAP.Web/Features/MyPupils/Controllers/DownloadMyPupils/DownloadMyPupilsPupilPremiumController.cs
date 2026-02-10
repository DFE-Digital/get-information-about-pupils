using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DownloadMyPupils;
[Route(Routes.MyPupilList.MyPupilsBase)]
public class DownloadMyPupilsPupilPremiumController : Controller
{
    private readonly ILogger<DownloadMyPupilsNationalPupilDatabaseController> _logger;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IGetSelectedPupilsUniquePupilNumbersPresentationService _getSelectedPupilsPresentationHandler;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsPupilSelectionsCommandHandler;

    public DownloadMyPupilsPupilPremiumController(
        ILogger<DownloadMyPupilsNationalPupilDatabaseController> logger,
        IMyPupilsMessageSink myPupilsLogSink,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectedPupilsPresentationHandler,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsPupilSelectionsCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(getSelectedPupilsPresentationHandler);
        _getSelectedPupilsPresentationHandler = getSelectedPupilsPresentationHandler;

        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupilsService);
        _downloadPupilPremiumDataForPupilsService = downloadPupilPremiumDataForPupilsService;

        ArgumentNullException.ThrowIfNull(updateMyPupilsPupilSelectionsCommandHandler);
        _updateMyPupilsPupilSelectionsCommandHandler = updateMyPupilsPupilSelectionsCommandHandler;
    }

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        MyPupilsFormStateRequestDto? updateForm,
        MyPupilsQueryRequestDto? query,
        CancellationToken ctx = default)
    {
        query ??= new();
        List<string> updatedPupils = await UpsertSelectedPupilsAsync(updateForm);

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
