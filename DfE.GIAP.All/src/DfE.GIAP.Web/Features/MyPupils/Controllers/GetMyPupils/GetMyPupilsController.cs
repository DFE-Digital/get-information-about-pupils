using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Areas.GetMyPupils;

[Route(Constants.Routes.MyPupilList.MyPupilsBase)]
public class GetMyPupilsController : Controller
{
    private readonly ILogger<GetMyPupilsController> _logger;
    private readonly IMyPupilsPresentationService _myPupilsPresentationService;
    private readonly IMapper<MyPupilsPresentationResponse, MyPupilsViewModel> _mapToPupilsViewModel;

    public GetMyPupilsController(
        ILogger<GetMyPupilsController> logger,
        IMyPupilsPresentationService myPupilsPresentationService,
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
        _logger.LogInformation("{Controller}.{Action} GET method is called", nameof(GetMyPupilsController), nameof(Index));

        MyPupilsPresentationResponse response =
            await _myPupilsPresentationService.GetPupils(User.GetUserId(), query);

        MyPupilsViewModel viewModel = _mapToPupilsViewModel.Map(response);

        return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}
