using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.UpdatePupilSelections;
using DfE.GIAP.Web.Helpers.Search;
using Microsoft.AspNetCore.Mvc;
using MessageLevel = DfE.GIAP.Web.Features.MyPupils.Messaging.MessageLevel;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

[Route(Constants.Routes.MyPupilList.MyPupilsBase)]
public class UpdateMyPupilsFormController : Controller
{
    private readonly ILogger<UpdateMyPupilsFormController> _logger;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsStateCommandHandler;

    public UpdateMyPupilsFormController(
        ILogger<UpdateMyPupilsFormController> logger,
        IMyPupilsMessageSink myPupilsLogSink,
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsStateCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(updateMyPupilsStateCommandHandler);
        _updateMyPupilsStateCommandHandler = updateMyPupilsStateCommandHandler;
    }

    [HttpPost]
    // Prevent browser-caching from back button presenting stale state
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [ValidateAntiForgeryToken]
    public IActionResult Index(MyPupilsFormStateRequestDto formDto, MyPupilsQueryRequestDto query)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(UpdateMyPupilsFormController), nameof(Index));

        if (!ModelState.IsValid)
        {
            _myPupilsLogSink.Add(
                new MyPupilsMessage(
                    level: MessageLevel.Error,
                    message: PupilHelper.GenerateValidationMessageUpnSearch(ModelState)));

            return RedirectToGetMyPupils(query);
        }

        _updateMyPupilsStateCommandHandler.Handle(formDto);

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
