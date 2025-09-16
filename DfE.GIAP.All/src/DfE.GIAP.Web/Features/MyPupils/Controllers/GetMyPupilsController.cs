using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.MyPupilList.MyPupils)]
public class GetMyPupilsController : Controller
{
    private readonly ILogger<GetMyPupilsController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;

    public GetMyPupilsController(
        ILogger<GetMyPupilsController> logger,
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

        MyPupilsViewModelContext context = new(error: null)
        {
            IsDeleteSuccessful = TempData.TryGetValue("IsDeleteSuccessful", out _)
        };

        MyPupilsViewModel viewModel =
            await _myPupilsViewModelFactory.CreateViewModelAsync(userId: User.GetUserId(), context);

        return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}

public record MyPupilsViewModelContext
{
    public MyPupilsViewModelContext(MyPupilsErrorViewModel? error)
    {
        Error = error ?? MyPupilsErrorViewModel.NOOP();
    }

    public bool IsDeleteSuccessful { get; init; } = false;
    public MyPupilsErrorViewModel Error { get; init; }
}
