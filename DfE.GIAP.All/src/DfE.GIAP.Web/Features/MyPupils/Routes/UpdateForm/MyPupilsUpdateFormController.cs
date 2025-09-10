using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Helpers.Search;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;

[Route(Constants.Routes.Application.MyPupilList)]
public class MyPupilsUpdateFormController : Controller
{
    private readonly ILogger<MyPupilsUpdateFormController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly IUpdateMyPupilsStateHandler _updateMyPupilsStateHandler;

    public MyPupilsUpdateFormController(
        ILogger<MyPupilsUpdateFormController> logger,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        IUpdateMyPupilsStateHandler updateMyPupilsStateHandler,
        IMyPupilsViewModelFactory myPupilsViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsViewModelFactory);
        _myPupilsViewModelFactory = myPupilsViewModelFactory;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(updateMyPupilsStateHandler);
        _updateMyPupilsStateHandler = updateMyPupilsStateHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Index(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("My pupil list POST method called");

        UserId userId = new(User.GetUserId());

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));

            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, error);

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        await _updateMyPupilsStateHandler.HandleAsync(
            new UpdateMyPupilsStateRequest(
                UserId: userId,
                State: state,
                UpdateStateInput: formDto));

        return RedirectToAction(actionName: "Index", controllerName: "MyPupils"); // TODO reflect name and strip Controller in extension
    }
}
