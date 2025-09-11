using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;
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

[Route(Constants.Routes.Application.MyPupilList)]
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
        _logger.LogInformation("My pupil list POST method called");

        UserId userId = new(User.GetUserId());

        if (!ModelState.IsValid)
        {
            MyPupilsErrorViewModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));

            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, error);

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        // Update SelectionState
        PaginatedMyPupilsResponse currentPageOfPupilsResponse = await _getPaginatedMyPupilsHandler.HandleAsync(
            new GetPaginatedMyPupilsRequest(
                userId,
                state.PresentationState));

        MyPupilsFormSelectionModeRequestDto selectAllMode =
            !formDto.SelectAll.HasValue ? MyPupilsFormSelectionModeRequestDto.ManualSelection :
                formDto.SelectAll.Value ? MyPupilsFormSelectionModeRequestDto.SelectAll :
                    MyPupilsFormSelectionModeRequestDto.DeselectAll;

        Action<MyPupilsPupilSelectionState> updateStrategy = GetSelectionStateUpdateStrategy(
            selectAllMode,
            currentPageOfPupilsResponse.Pupils.Identifiers.ToList(),
            formDto.SelectedPupils);

        updateStrategy.Invoke(state.SelectionState);

        _pupilSelectionStateCommandHandler.StoreInSession(state.SelectionState);

        // Update PresentationState
        MyPupilsPresentationState updatedPresentationState = new(
                Page: formDto.PageNumber,
                SortBy: formDto.SortField,
                SortDirection: formDto.SortDirection == "asc" ?
                    SortDirection.Ascending :
                        SortDirection.Descending);

        _presentationStateCommandHandler.StoreInSession(updatedPresentationState);

        return RedirectToAction(actionName: "Index", controllerName: "MyPupils"); // TODO reflect name and strip Controller in extension
    }

    private static Action<MyPupilsPupilSelectionState> GetSelectionStateUpdateStrategy(
        MyPupilsFormSelectionModeRequestDto mode,
        List<UniquePupilNumber> currentPageOfPupils,
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
                List<UniquePupilNumber> selectedPupils = selectedPupilsOnForm?.ToUniquePupilNumbers().ToList() ?? [];
                List<UniquePupilNumber> deselectedPupils = currentPageOfPupils.Except(selectedPupils).ToList();

                state.UpsertPupilSelectionState(selectedPupils, true);
                state.UpsertPupilSelectionState(deselectedPupils, false);
            }
        };
        return selectionStateHandler;
    }
}
