using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
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
public class DeleteMyPupilsController : Controller
{
    private readonly ILogger<DeleteMyPupilsController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IUseCaseRequestOnly<DeleteAllMyPupilsRequest> _deleteAllPupilsUseCase;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsFromMyPupilsUseCase;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;
    private readonly IGetPupilViewModelsHandler _getPupilViewModelsForUserHandler;

    public DeleteMyPupilsController(
        ILogger<DeleteMyPupilsController> logger,
        IMyPupilsViewModelFactory myPupilsViewModelFactory,
        IUseCaseRequestOnly<DeleteAllMyPupilsRequest> deleteAllPupilsUseCase,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deleteMyPupilsUseCase,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetPupilViewModelsHandler getPupilViewModelsForUserHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsViewModelFactory);
        _myPupilsViewModelFactory = myPupilsViewModelFactory;

        ArgumentNullException.ThrowIfNull(deleteAllPupilsUseCase);
        _deleteAllPupilsUseCase = deleteAllPupilsUseCase;

        ArgumentNullException.ThrowIfNull(deleteMyPupilsUseCase);
        _deletePupilsFromMyPupilsUseCase = deleteMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;

        ArgumentNullException.ThrowIfNull(getPupilViewModelsForUserHandler);
        _getPupilViewModelsForUserHandler = getPupilViewModelsForUserHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] List<string> SelectedPupils)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(DeleteMyPupilsController), nameof(Delete));

        string userId = User.GetUserId();

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        PupilsViewModel currentPagePupilViewModels =
            await _getPupilViewModelsForUserHandler.GetPupilsAsync(
                new GetPupilViewModelsRequest(userId, state));

        if (!ModelState.IsValid)
        {
            return View(
                Constants.Routes.MyPupilList.MyPupilListView,
                model: _myPupilsViewModelFactory.CreateViewModel(
                            state,
                            currentPagePupilViewModels,
                            error: PupilHelper.GenerateValidationMessageUpnSearch(ModelState)));
        }

        bool noSelectedPupils = SelectedPupils.Count == 0 && !state.SelectionState.IsAnyPupilSelected;

        if (noSelectedPupils)
        {
            return View(
                Constants.Routes.MyPupilList.MyPupilListView,
                model: _myPupilsViewModelFactory.CreateViewModel(
                            state,
                            currentPagePupilViewModels,
                            error: Messages.Common.Errors.NoPupilsSelected));
        }

        // If the client deselects one or more pupils when SelectAll is active, we should not also delete the DeselectedPupils.
        // We infer if ALL pupils on the page are selected, by matching counts
        bool isDeleteAllPupils =
            state.SelectionState.IsAllPupilsSelected &&
                currentPagePupilViewModels.Count.Equals(SelectedPupils.Count);

        if (isDeleteAllPupils)
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

        TempData["IsDeleteSuccessful"] = true;
        _selectionStateSessionCommandHandler.StoreInSession(state.SelectionState);

        return RedirectToAction(actionName: "Index", controllerName: "GetMyPupils");
    }
}
