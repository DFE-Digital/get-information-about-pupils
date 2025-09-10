using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Session.Abstraction.Command;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.Application.MyPupilList)]
public class MyPupilsDeletePupilsController : Controller
{
    private readonly ILogger<MyPupilsDeletePupilsController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsPresentationService;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deleteMyPupilsUseCase;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;

    public MyPupilsDeletePupilsController(
        ILogger<MyPupilsDeletePupilsController> logger,
        IMyPupilsViewModelFactory myPupilsPresentationService,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deleteMyPupilsUseCase,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;

        ArgumentNullException.ThrowIfNull(deleteMyPupilsUseCase);
        _deleteMyPupilsUseCase = deleteMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] bool? SelectAll, [FromForm] List<string> SelectedPupils)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        UserId user = new(User.GetUserId());

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            MyPupilsViewModel viewModel = await _myPupilsPresentationService.CreateViewModelAsync(user, error); 

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        if (SelectedPupils.Count == 0 && !SelectAll.HasValue) // TODO push behind the ModelState.IsValid
        {
            MyPupilsErrorViewModel noPupilSelected = new(Messages.Common.Errors.NoPupilsSelected);
            MyPupilsViewModel viewModel = await _myPupilsPresentationService.CreateViewModelAsync(user, noPupilSelected);

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        bool deleteAllPupils = SelectAll.HasValue && SelectAll.Value;

        DeletePupilsFromMyPupilsRequest deleteRequest = new(
                UserId: user,
                DeletePupilUpns: SelectedPupils.ToUniquePupilNumbers(),
                DeleteAll: deleteAllPupils);

        await _deleteMyPupilsUseCase.HandleRequestAsync(deleteRequest);

        UpdateSelectionState(request: deleteRequest, state: _getMyPupilsStateProvider.GetState().SelectionState);

        return RedirectToAction(actionName: "Index", controllerName: "MyPupils");
    }

    private void UpdateSelectionState(DeletePupilsFromMyPupilsRequest request, MyPupilsPupilSelectionState state)
    {
        if (request.DeleteAll)
        {
            state.ResetState();
        }
        else
        {
            state.RemovePupils(request.DeletePupilUpns);
        }

        _selectionStateSessionCommandHandler.StoreInSession(state);
    }
}
