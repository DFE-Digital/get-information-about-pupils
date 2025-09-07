using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;

internal sealed class UpdateMyPupilsStateHandler : IUpdateMyPupilsStateHandler
{
    private readonly IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> _formStateDtoToPresentationStateMapper;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateSessionComandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsHandler;

    public UpdateMyPupilsStateHandler(
        IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> formDtoToPresentationStateMapper,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateSessionComandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsHandler)
    {
        ArgumentNullException.ThrowIfNull(formDtoToPresentationStateMapper);
        _formStateDtoToPresentationStateMapper = formDtoToPresentationStateMapper;

        ArgumentNullException.ThrowIfNull(presentationStateSessionComandHandler);
        _presentationStateSessionComandHandler = presentationStateSessionComandHandler;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;

        ArgumentNullException.ThrowIfNull(_getPaginatedMyPupilsHandler);
        _getPaginatedMyPupilsHandler = getPaginatedMyPupilsHandler;
    }

    public async Task Handle(UpdateMyPupilsStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.State);
        ArgumentNullException.ThrowIfNull(request.UpdateStateInput);

        // Update SelectionState
        MyPupilsPupilSelectionState pupilSelectionState = request.State.SelectionState;

        PaginatedMyPupilsResponse paginatedMyPupilsResponse = await _getPaginatedMyPupilsHandler.HandleAsync(
            new GetPaginatedMyPupilsRequest(
                request.UserId,
                request.State.PresentationState));

        IReadOnlyList<UniquePupilNumber> currentPageOfPupils = paginatedMyPupilsResponse.Pupils.Identifiers;

        if (request.UpdateStateInput.SelectAllState == MyPupilsFormSelectionStateRequestDto.SelectAll)
        {
            pupilSelectionState.UpsertPupilSelectionState(currentPageOfPupils, isSelected: true);
            pupilSelectionState.SelectAllPupils();
        }
        else if (request.UpdateStateInput.SelectAllState == MyPupilsFormSelectionStateRequestDto.DeselectAll)
        {
            pupilSelectionState.UpsertPupilSelectionState(currentPageOfPupils, isSelected: false);
            pupilSelectionState.DeselectAllPupils();
        }
        else
        {
            // Manually track selections/deselections
            IEnumerable<UniquePupilNumber> selectedPupils = request.UpdateStateInput.SelectedPupils.ToUniquePupilNumbers() ?? [];
            IEnumerable<UniquePupilNumber> deselectedPupils = currentPageOfPupils.Except(selectedPupils);

            pupilSelectionState.UpsertPupilSelectionState(selectedPupils, isSelected: true);
            pupilSelectionState.UpsertPupilSelectionState(deselectedPupils, isSelected: false);
        }

        _selectionStateSessionCommandHandler.StoreInSession(pupilSelectionState);

        // Update PresentationState
        MyPupilsPresentationState myPupilsPresentationState = _formStateDtoToPresentationStateMapper.Map(request.UpdateStateInput);
        _presentationStateSessionComandHandler.StoreInSession(myPupilsPresentationState);
    }
}
