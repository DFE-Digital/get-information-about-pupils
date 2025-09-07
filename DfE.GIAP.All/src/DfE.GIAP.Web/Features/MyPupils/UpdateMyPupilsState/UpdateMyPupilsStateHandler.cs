using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;

internal sealed class UpdateMyPupilsStateHandler : IUpdateMyPupilsStateHandler
{
    private readonly IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> _formStateDtoToPresentationStateMapper;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateSessionComandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;

    public UpdateMyPupilsStateHandler(
        IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> formDtoToPresentationStateMapper,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateSessionComandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(formDtoToPresentationStateMapper);
        _formStateDtoToPresentationStateMapper = formDtoToPresentationStateMapper;

        ArgumentNullException.ThrowIfNull(presentationStateSessionComandHandler);
        _presentationStateSessionComandHandler = presentationStateSessionComandHandler;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;
    }

    public void Handle(UpdateMyPupilsStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.State);
        ArgumentNullException.ThrowIfNull(request.UpdateStateInput);

        // Update PresentationState
        MyPupilsPresentationState myPupilsPresentationState = _formStateDtoToPresentationStateMapper.Map(request.UpdateStateInput);
        _presentationStateSessionComandHandler.StoreInSession(myPupilsPresentationState);

        // Update SelectionState
        MyPupilsPupilSelectionState pupilSelectionState = request.State.SelectionState;

        if (request.UpdateStateInput.SelectAllState == MyPupilsFormSelectionStateRequestDto.SelectAll)
        {
            pupilSelectionState.UpsertPupilSelectionState(pupilSelectionState.CurrentPageOfPupils, isSelected: true);
            pupilSelectionState.SelectAllPupils();
        }
        else if (request.UpdateStateInput.SelectAllState == MyPupilsFormSelectionStateRequestDto.DeselectAll)
        {
            pupilSelectionState.UpsertPupilSelectionState(pupilSelectionState.CurrentPageOfPupils, isSelected: false);
            pupilSelectionState.DeselectAllPupils();
        }
        else
        {
            // Manually track selections/deselections
            IEnumerable<UniquePupilNumber> selectedPupils = request.UpdateStateInput.SelectedPupils.ToUniquePupilNumbers() ?? [];
            IEnumerable<UniquePupilNumber> deselectedPupils = pupilSelectionState.CurrentPageOfPupils.Except(selectedPupils);

            pupilSelectionState.UpsertPupilSelectionState(selectedPupils, isSelected: true);
            pupilSelectionState.UpsertPupilSelectionState(deselectedPupils, isSelected: false);
        }

        _selectionStateSessionCommandHandler.StoreInSession(pupilSelectionState);
    }
}
