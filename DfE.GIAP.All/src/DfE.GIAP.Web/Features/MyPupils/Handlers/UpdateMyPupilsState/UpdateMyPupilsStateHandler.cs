using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;
using DfE.GIAP.Web.Features.MyPupils.PresentationState.Provider;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.UpdateMyPupilsState;

public sealed class UpdateMyPupilsStateHandler : IUpdateMyPupilsStateHandler
{
    private readonly IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> _formDtoToPresentationStateMapper;
    private readonly IMyPupilsPresentationStateProvider _presentPupilOptionsProvider;
    private readonly IMyPupilsPupilSelectionStateProvider _pupilSelectionStateProvider;
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsHandler;

    public UpdateMyPupilsStateHandler(
        IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> formStateToPresentationStateMapper,
        IMyPupilsPresentationStateProvider presentPupilOptionsProvider,
        IMyPupilsPupilSelectionStateProvider pupilSelectionStateProvider,
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsHandler)
    {
        ArgumentNullException.ThrowIfNull(formStateToPresentationStateMapper);
        _formDtoToPresentationStateMapper = formStateToPresentationStateMapper;

        ArgumentNullException.ThrowIfNull(presentPupilOptionsProvider);
        _presentPupilOptionsProvider = presentPupilOptionsProvider;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateProvider);
        _pupilSelectionStateProvider = pupilSelectionStateProvider;

        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsHandler);
        _getPaginatedMyPupilsHandler = getPaginatedMyPupilsHandler;
    }

    public async Task HandleAsync(UpdateMyPupilsStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        GetPaginatedMyPupilsRequest getPaginatedMyPupilsRequest = new(request.UserId, request.CurrentPresentationState);

        IEnumerable<string> currentPageOfPupils =
            (await _getPaginatedMyPupilsHandler.GetPaginatedPupilsAsync(getPaginatedMyPupilsRequest)).Pupils
                .Select(t => t.UniquePupilNumber);
        
        _presentPupilOptionsProvider.SetState(
            state: _formDtoToPresentationStateMapper.Map(request.UpdateStateInput));

        UpdatePupilSelectionState(
            selectAllState: request.UpdateStateInput.SelectAllState,
            currentPageOfPupils,
            selectedPupilsOnPage: request.UpdateStateInput.SelectedPupils ?? Enumerable.Empty<string>());
    }

    private void UpdatePupilSelectionState(
        MyPupilsFormSelectAllStateRequestDto selectAllState,
        IEnumerable<string> currentPageOfPupils,
        IEnumerable<string> selectedPupilsOnPage)
    {
        MyPupilsPupilSelectionState currentState = _pupilSelectionStateProvider.GetState();

        if (selectAllState == MyPupilsFormSelectAllStateRequestDto.SelectAll)
        {
            currentState.UpsertPupilWithSelectedState(currentPageOfPupils, isSelected: true);
            currentState.SelectAllPupils();
        }
        else if (selectAllState == MyPupilsFormSelectAllStateRequestDto.DeselectAll)
        {
            currentState.UpsertPupilWithSelectedState(currentPageOfPupils, isSelected: false);
            currentState.DeselectAllPupils();
        }
        else
        {
            IEnumerable<string> deselectedOnPage = currentPageOfPupils.Except(selectedPupilsOnPage);

            currentState.UpsertPupilWithSelectedState(selectedPupilsOnPage, isSelected: true);
            currentState.UpsertPupilWithSelectedState(deselectedOnPage, isSelected: false);
        }

        _pupilSelectionStateProvider.SetState(currentState);
    }
}
