using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;

[Route(Constants.Routes.MyPupilList.MyPupilsBase)]
public class GetMyPupilsController : Controller
{
    private readonly ILogger<GetMyPupilsController> _logger;
    private readonly IGetMyPupilsPresentationService _myPupilsPresentationService;
    private readonly IMapper<MyPupilsPresentationResponse, MyPupilsViewModel> _mapToPupilsViewModel;

    public GetMyPupilsController(
        ILogger<GetMyPupilsController> logger,
        IGetMyPupilsPresentationService myPupilsPresentationService,
        IMapper<MyPupilsPresentationResponse, MyPupilsViewModel> mapToPupilsViewModel)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;

        ArgumentNullException.ThrowIfNull(mapToPupilsViewModel);
        _mapToPupilsViewModel = mapToPupilsViewModel;
    }

    [HttpGet]
    // Prevent browser-caching e.g. back button presenting stale state
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(MyPupilsQueryRequestDto query)
    {
        _logger.LogInformation("{Controller}.{Action} GET called", nameof(GetMyPupilsController), nameof(Index));

        MyPupilsPresentationResponse response =
            await _myPupilsPresentationService.GetPupilsAsync(
                User.GetUserId(),
                query);

        MyPupilsViewModel viewModel = _mapToPupilsViewModel.Map(response);

        return View(Constants.Routes.MyPupilList.MyPupilsView, viewModel);
    }
}
