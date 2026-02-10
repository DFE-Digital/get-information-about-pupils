using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Helpers.Search;
using Microsoft.AspNetCore.Mvc;
using MessageLevel = DfE.GIAP.Web.Features.MyPupils.Messaging.MessageLevel;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

[Route(Constants.Routes.MyPupilList.MyPupilsBase)]
public class UpdateMyPupilsController : Controller
{
    private readonly ILogger<UpdateMyPupilsController> _logger;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsSelectionsCommandHandler;

    public UpdateMyPupilsController(
        ILogger<UpdateMyPupilsController> logger,
        IMyPupilsMessageSink myPupilsLogSink,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsStateCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(updateMyPupilsStateCommandHandler);
        _updateMyPupilsSelectionsCommandHandler = updateMyPupilsStateCommandHandler;
    }

    [HttpPost]
    // Prevent browser-caching from back button presenting stale state
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [ValidateAntiForgeryToken]
    public IActionResult Index(MyPupilsPupilSelectionsRequestDto selectionsDto, MyPupilsQueryRequestDto query)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(UpdateMyPupilsController), nameof(Index));

        query ??= new();

        if (selectionsDto is null || !ModelState.IsValid)
        {
            _myPupilsLogSink.AddMessage(
                new MyPupilsMessage(
                    level: MessageLevel.Error,
                    message: PupilHelper.GenerateValidationMessageUpnSearch(ModelState)));

            return MyPupilsRedirectHelpers.RedirectToGetMyPupils(query);
        }

        _updateMyPupilsSelectionsCommandHandler.Handle(selectionsDto);

        return MyPupilsRedirectHelpers.RedirectToGetMyPupils(query);
    }
}
