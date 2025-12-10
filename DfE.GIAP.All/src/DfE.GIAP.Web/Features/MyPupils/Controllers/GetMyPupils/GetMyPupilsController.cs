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

    public GetMyPupilsController(
        ILogger<GetMyPupilsController> logger,
        IMyPupilsPresentationService myPupilsPresentationService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(MyPupilsQueryRequestDto request)
    {
        _logger.LogInformation("{Controller}.{Action} GET method is called", nameof(GetMyPupilsController), nameof(Index));

        MyPupilsPresentationResponse response =
            await _myPupilsPresentationService.GetPupils(
                userId: User.GetUserId(),
                request.PageNumber,
                request.SortField,
                request.SortDirection);

        // TODO IMapper<MyPupilsPresentationResponseModel -> MyPupilsViewModel>
        MyPupilsViewModel viewModel = new()
        {
            Pupils = response.MyPupils,
            PageNumber = response.PageNumber,
            SortDirection = response.SortedDirection,
            SortField = response.SortedField,
            IsAnyPupilsSelected = response.IsAnyPupilsSelected
            //IsDeleteSuccessful = guardedContext.IsDeletePupilsSuccessful,
            //Error = string.IsNullOrEmpty(guardedContext.Error) ? MyPupilsErrorViewModel.NOOP() : MyPupilsErrorViewModel.Create(guardedContext.Error)
        };

        return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}
