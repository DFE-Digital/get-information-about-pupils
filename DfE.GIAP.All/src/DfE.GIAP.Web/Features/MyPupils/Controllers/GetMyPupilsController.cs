using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.MyPupilList.MyPupils)]
public class GetMyPupilsController : Controller
{
    private readonly ILogger<GetMyPupilsController> _logger;
    private readonly IGetMyPupilsStateProvider _stateProvider;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IGetPupilViewModelsHandler _getPupilViewModelsForUserHandler;

    public GetMyPupilsController(
        ILogger<GetMyPupilsController> logger,
        IMyPupilsViewModelFactory viewModelFactory,
        IGetMyPupilsStateProvider stateProvider,
        IGetPupilViewModelsHandler getPupilViewModelsHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(viewModelFactory);
        _myPupilsViewModelFactory = viewModelFactory;

        ArgumentNullException.ThrowIfNull(stateProvider);
        _stateProvider = stateProvider;

        ArgumentNullException.ThrowIfNull(getPupilViewModelsHandler);
        _getPupilViewModelsForUserHandler = getPupilViewModelsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("{Controller}.{Action} GET method is called", nameof(GetMyPupilsController), nameof(Index));

        string userId = User.GetUserId();

        MyPupilsState state = _stateProvider.GetState();

        PupilsViewModel pupilViewModels =
            await _getPupilViewModelsForUserHandler.GetPupilsAsync(
                new GetPupilViewModelsRequest(userId, state));

        MyPupilsViewModel viewModel =
            _myPupilsViewModelFactory.CreateViewModel(
                state,
                pupilViewModels,
                context: new MyPupilsViewModelContext(isDeletePupilsSucessful: TempData.TryGetValue("IsDeleteSuccessful", out _)));

        return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}
