using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler;
using DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;
using DfE.GIAP.Web.Session.Abstraction.Command;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;

#nullable enable

[Route(Constants.Routes.MyPupilList.DeleteMyPupils)]
public class DeleteMyPupilsController : Controller
{
    private readonly ILogger<DeleteMyPupilsController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IUseCaseRequestOnly<DeleteAllMyPupilsRequest> _deleteAllPupilsUseCase;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsFromMyPupilsUseCase;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;
    private readonly IGetMyPupilsHandler _getPupilViewModelsForUserHandler;

    public DeleteMyPupilsController(
        ILogger<DeleteMyPupilsController> logger,
        IMyPupilsViewModelFactory viewModelFactory,
        IUseCaseRequestOnly<DeleteAllMyPupilsRequest> deleteAllPupilsUseCase,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deleteSomePupilsUseCase,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetMyPupilsHandler getPupilViewModelsHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(viewModelFactory);
        _myPupilsViewModelFactory = viewModelFactory;

        ArgumentNullException.ThrowIfNull(deleteAllPupilsUseCase);
        _deleteAllPupilsUseCase = deleteAllPupilsUseCase;

        ArgumentNullException.ThrowIfNull(deleteSomePupilsUseCase);
        _deletePupilsFromMyPupilsUseCase = deleteSomePupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;

        ArgumentNullException.ThrowIfNull(getPupilViewModelsHandler);
        _getPupilViewModelsForUserHandler = getPupilViewModelsHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] List<string> SelectedPupils)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(DeleteMyPupilsController), nameof(Delete));

        string userId = User.GetUserId();

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        MyPupilsResponse response =
            await _getPupilViewModelsForUserHandler.GetPupilsAsync(
                new MyPupilsRequest(userId, state));

        MyPupilsPresentationModel currentPagePupilViewModels = response.MyPupils;

        if (TryEvaluateErrorViewResult(conditionsToErrorMessages: new()
            {
                { () => !ModelState.IsValid ,
                    "There has been a problem with selections. Please try again." },
                { () => SelectedPupils.Count == 0 && !state.SelectionState.IsAnyPupilSelected,
                    Messages.Common.Errors.NoPupilsSelected }
            }, state, currentPagePupilViewModels, out ViewResult? errorViewResult))
        {
            return errorViewResult;
        }


        // If the client deselects one or more pupils when SelectAll is active, we should not also delete the DeselectedPupils.
        // We infer if ALL pupils on the page are selected, by matching counts
        bool isAllPupilsBeingDeleted =
            state.SelectionState.IsAllPupilsSelected &&
                currentPagePupilViewModels.Count.Equals(SelectedPupils.Count);

        if (isAllPupilsBeingDeleted)
        {
            await _deleteAllPupilsUseCase.HandleRequestAsync(
                new DeleteAllMyPupilsRequest(userId));

            state.SelectionState.ResetState();
        }

        else
        {
            List<string> deletePupilUpns =
                SelectedPupils.AsEnumerable()
                    .Concat(state.SelectionState.GetSelectedPupils())
                    .ToList();

            await _deletePupilsFromMyPupilsUseCase.HandleRequestAsync(
                new DeletePupilsFromMyPupilsRequest(
                    UserId: userId,
                    DeletePupilUpns: deletePupilUpns));

            state.SelectionState.RemovePupils(deletePupilUpns);
        }

        _selectionStateSessionCommandHandler.StoreInSession(state.SelectionState);
        TempData["IsDeleteSuccessful"] = true;
        return RedirectToAction(actionName: "Index", controllerName: "GetMyPupils");
    }

    private bool TryEvaluateErrorViewResult(
        Dictionary<Func<bool>, string> conditionsToErrorMessages,
        MyPupilsState state,
        MyPupilsPresentationModel pupilViewModels,
        out ViewResult? result)
    {

        foreach (KeyValuePair<Func<bool>, string> condition in conditionsToErrorMessages)
        {
            if (condition.Key.Invoke())
            {
                result = View(
                        viewName: Constants.Routes.MyPupilList.MyPupilListView,
                        model: _myPupilsViewModelFactory.CreateViewModel(
                                state,
                                pupilViewModels,
                                MyPupilsViewModelContext.CreateWithErrorMessage(condition.Value)));
                return true;
            }
        }

        result = null;
        return false;
    }
}
