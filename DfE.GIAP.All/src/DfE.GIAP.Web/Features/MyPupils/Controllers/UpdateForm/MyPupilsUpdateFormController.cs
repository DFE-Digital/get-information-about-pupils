using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Session.Abstraction.Command;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;

namespace DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;

[Route(Constants.Routes.MyPupilList.MyPupils)]
public class MyPupilsUpdateFormController : Controller
{
    private readonly ILogger<MyPupilsUpdateFormController> _logger;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateCommandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsHandler;

    public MyPupilsUpdateFormController(
        ILogger<MyPupilsUpdateFormController> logger,
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        IMyPupilsViewModelFactory myPupilsViewModelFactory,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateCommandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler,
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsHandler)
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

        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsHandler);
        _getPaginatedMyPupilsHandler = getPaginatedMyPupilsHandler;
        
    }

    [HttpPost]
    public async Task<IActionResult> Index(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(MyPupilsUpdateFormController), nameof(Index));

        string userId = User.GetUserId();

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = MyPupilsErrorViewModel.Create(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            MyPupilsViewModelContext context = new(error);
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, context);

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        // Update SelectionState
        PaginatedMyPupilsResponse currentPageOfPupilsResponse = await _getPaginatedMyPupilsHandler.HandleAsync(
            new GetPaginatedMyPupilsRequest(
                userId,
                state.PresentationState));

        GetSelectionStateUpdateStrategy(
                formDto.SelectAllMode, currentPageOfPupilsResponse.Pupils.Identifiers.ToList(), formDto.SelectedPupils)
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
