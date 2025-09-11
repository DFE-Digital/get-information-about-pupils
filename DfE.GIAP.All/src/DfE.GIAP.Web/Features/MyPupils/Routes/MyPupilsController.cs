using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.Application.MyPupilList)]
public class MyPupilsController : Controller
{
    private readonly ILogger<MyPupilsController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;

    public MyPupilsController(
        ILogger<MyPupilsController> logger,
        IMyPupilsViewModelFactory myPupilsPresentationService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsViewModelFactory = myPupilsPresentationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("My pupil list GET method is called");

        UserId userId = new(id: User.GetUserId());

        MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId);

        return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}
