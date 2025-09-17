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
    private readonly IGetPupilViewModelsHandler _getPupilViewModelsForUserHandler;
    private readonly IGetMyPupilsStateProvider _stateProvider;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;

    public GetMyPupilsController(
        ILogger<GetMyPupilsController> logger,
        IMyPupilsViewModelFactory myPupilsPresentationService,
        IGetMyPupilsStateProvider stateProvider,
        IGetPupilViewModelsHandler getPupilViewModelsForUserHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsViewModelFactory = myPupilsPresentationService;
        _stateProvider = stateProvider;
        _getPupilViewModelsForUserHandler = getPupilViewModelsForUserHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("My pupil list GET method is called");

        string userId = User.GetUserId();

        MyPupilsState state = _stateProvider.GetState();

        PupilsViewModel pupilViewModels =
            await _getPupilViewModelsForUserHandler.GetPupilsAsync(
                new GetPupilViewModelsRequest(userId, state));

        MyPupilsViewModel viewModel = _myPupilsViewModelFactory.CreateViewModel(
            state,
            pupilViewModels,
            error: null,
            isDeleteSuccessful: TempData.TryGetValue("IsDeleteSuccessful", out _));

        return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}
