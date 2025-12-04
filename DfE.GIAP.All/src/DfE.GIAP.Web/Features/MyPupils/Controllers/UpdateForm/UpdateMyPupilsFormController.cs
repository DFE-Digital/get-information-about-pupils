using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler;
using DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Session.Abstraction.Command;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;

namespace DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;

[Route(Constants.Routes.MyPupilList.MyPupils)]
public class UpdateMyPupilsFormController : Controller
{
    private readonly ILogger<UpdateMyPupilsFormController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateCommandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    private readonly IGetMyPupilsHandler _getPupilViewModelsForUserHandler;

    public UpdateMyPupilsFormController(
        ILogger<UpdateMyPupilsFormController> logger,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        IMyPupilsViewModelFactory myPupilsViewModelFactory,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateCommandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler,
        IGetMyPupilsHandler getPupilViewModelsForUserHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsViewModelFactory);
        _myPupilsViewModelFactory = myPupilsViewModelFactory;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(presentationStateCommandHandler);
        _presentationStateCommandHandler = presentationStateCommandHandler;

        ArgumentNullException.ThrowIfNull(presentationStateCommandHandler);
        _pupilSelectionStateCommandHandler = pupilSelectionStateCommandHandler;

        ArgumentNullException.ThrowIfNull(getPupilViewModelsForUserHandler);
        _getPupilViewModelsForUserHandler = getPupilViewModelsForUserHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Index(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(UpdateMyPupilsFormController), nameof(Index));

        string userId = User.GetUserId();

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        MyPupilsResponse response =
                await _getPupilViewModelsForUserHandler.GetPupilsAsync(
                    new MyPupilsRequest(userId, state));

        if (!ModelState.IsValid)
        {
            return View(
                Constants.Routes.MyPupilList.MyPupilListView,
                model: _myPupilsViewModelFactory.CreateViewModel(
                            state,
                            response.MyPupils,
                            context: MyPupilsViewModelContext.CreateWithErrorMessage(PupilHelper.GenerateValidationMessageUpnSearch(ModelState))));
        }

        // Update SelectionState
        GetSelectionStateUpdateStrategy(
            mode: formDto.SelectAllState,
            currentPageOfPupils: response.MyPupils.Pupils.Select(t => t.UniquePupilNumber).ToList(),
            selectedPupilsOnForm: formDto.SelectedPupils)
                .Invoke(state.SelectionState);

        _pupilSelectionStateCommandHandler.StoreInSession(state.SelectionState);

        // Update PresentationState
        MyPupilsPresentationState updatedPresentationState = new(
                Page: formDto.PageNumber,
                SortBy: formDto.SortField,
                SortDirection: formDto.SortDirection == "asc" ?
                    SortDirection.Ascending :
                        SortDirection.Descending);

        _presentationStateCommandHandler.StoreInSession(updatedPresentationState);

        return RedirectToAction(actionName: "Index", controllerName: "GetMyPupils");
    }

    private static Action<MyPupilsPupilSelectionState> GetSelectionStateUpdateStrategy(
        MyPupilsFormSelectionModeRequestDto mode,
        List<string> currentPageOfPupils,
        List<string> selectedPupilsOnForm)
    {
        Action<MyPupilsPupilSelectionState> selectionStateHandler = mode switch
        {
            MyPupilsFormSelectionModeRequestDto.SelectAll => (state) =>
            {
                state.UpsertPupilSelectionState(currentPageOfPupils, true);
                state.SelectAllPupils();
            },
            MyPupilsFormSelectionModeRequestDto.DeselectAll => (state) =>
            {

                state.UpsertPupilSelectionState(currentPageOfPupils, false);
                state.DeselectAllPupils();
            },
            _ => (state) =>
            {
                List<string> selectedPupils = selectedPupilsOnForm?.ToList() ?? [];
                List<string> deselectedPupils = currentPageOfPupils.Except(selectedPupils).ToList();

                state.UpsertPupilSelectionState(selectedPupils, true);
                state.UpsertPupilSelectionState(deselectedPupils, false);
            }
        };
        return selectionStateHandler;
    }
}
