using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Logging;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Helpers.Search;
using Microsoft.AspNetCore.Mvc;
using LogLevel = DfE.GIAP.Web.Features.MyPupils.Logging.LogLevel;

namespace DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;

[Route(Constants.Routes.MyPupilList.MyPupilsBase)]
public class UpdateMyPupilsFormController : Controller
{
    private readonly ILogger<UpdateMyPupilsFormController> _logger;
    private readonly IMyPupilsLogSink _myPupilsLogSink;
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsStateCommandHandler;


    public UpdateMyPupilsFormController(
        ILogger<UpdateMyPupilsFormController> logger,
        IMyPupilsLogSink myPupilsLogSink,
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MyPupilsFormStateRequestDto formDto, MyPupilsQueryRequestDto query)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(UpdateMyPupilsFormController), nameof(Index));

        if (!ModelState.IsValid)
        {
            _myPupilsLogSink.Add(
                new MyPupilsLog(
                    level: LogLevel.Error,
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
                PageNumber = request.PageNumber,
                SortField = request.SortField,
                SortDirection = request.SortDirection
            });
    }
}
