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
    private readonly IUpdateMyPupilsStateCommandHandler _updateMyPupilsStateCommandHandler;


    public UpdateMyPupilsFormController(
        ILogger<UpdateMyPupilsFormController> logger,
        IMyPupilsLogSink myPupilsLogSink,
        IUpdateMyPupilsStateCommandHandler updateMyPupilsStateCommandHandler)
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
    public async Task<IActionResult> Index(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(UpdateMyPupilsFormController), nameof(Index));

        if (!ModelState.IsValid)
        {
            _myPupilsLogSink.Add(
                new MyPupilsLog(
                    level: LogLevel.Error,
                    message: PupilHelper.GenerateValidationMessageUpnSearch(ModelState)));

            return LocalRedirect(Constants.Routes.MyPupilList.MyPupilsBase);
        }

        _updateMyPupilsStateCommandHandler.Handle(formDto);

        return LocalRedirect(Constants.Routes.MyPupilList.MyPupilsBase);
    }
}
