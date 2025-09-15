using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Session.Abstraction.Command;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.MyPupilList.DeleteMyPupils)]
public class MyPupilsDeletePupilsController : Controller
{
    private readonly ILogger<MyPupilsDeletePupilsController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deleteMyPupilsUseCase;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;

    public MyPupilsDeletePupilsController(
        ILogger<MyPupilsDeletePupilsController> logger,
        IMyPupilsViewModelFactory myPupilsViewModelFactory,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deleteMyPupilsUseCase,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsViewModelFactory);
        _myPupilsViewModelFactory = myPupilsViewModelFactory;

        ArgumentNullException.ThrowIfNull(deleteMyPupilsUseCase);
        _deleteMyPupilsUseCase = deleteMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;

        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsHandler);
        _getPaginatedMyPupilsHandler = getPaginatedMyPupilsHandler;
    }

    // TODO consider Breaking up UseCase to DeleteAllPupilsFromMyPupilsUseCase and DeleteUniquePupilNumbersFromMyPupilsUseCase
    // TODO test deselection one, across pages

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] List<string> SelectedPupils, CancellationToken ctx)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        UserId userId = new(User.GetUserId());

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, error);

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        if (SelectedPupils.Count == 0
                && !state.SelectionState.IsAnyPupilSelected)
        {
            MyPupilsErrorViewModel noPupilSelected = new(Messages.Common.Errors.NoPupilsSelected);
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, noPupilSelected);

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        // If the client deselects one or more pupils when SelectAll is active, we should not delete the deselectedPupils on the page (i.e use UniquePupilNumbers).
        // We infer if ALL pupils on the page are selected, by matching counts
        bool isDeleteAllPupils =
            state.SelectionState.IsAllPupilsSelected &&
                (await _getPaginatedMyPupilsHandler.HandleAsync(new GetPaginatedMyPupilsRequest(userId, state.PresentationState)))
                    .Pupils.Count.Equals(SelectedPupils.Count);


        List <UniquePupilNumber> deletePupilUpns =
            (isDeleteAllPupils ?
                [] : SelectedPupils.ToUniquePupilNumbers().Concat(state.SelectionState.GetSelectedPupils()))
                    .ToList();
             
        DeletePupilsFromMyPupilsRequest deleteRequest = new(
                UserId: userId,
                DeletePupilUpns: deletePupilUpns,
                DeleteAll: isDeleteAllPupils,
                CancellationToken: CancellationToken.None);

        await _deleteMyPupilsUseCase.HandleRequestAsync(deleteRequest);

        UpdateSelectionState(
            deleteRequest,
            _getMyPupilsStateProvider.GetState().SelectionState);

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
