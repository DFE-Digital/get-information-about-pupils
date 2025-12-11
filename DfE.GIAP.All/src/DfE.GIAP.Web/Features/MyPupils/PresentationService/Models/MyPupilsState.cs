using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record MyPupilsState
{
    public MyPupilsState(MyPupilsPresentationQueryModel presentationState, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(presentationState);
        PresentationState = presentationState;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilsPresentationQueryModel PresentationState { get; init; }
    public MyPupilsPupilSelectionState SelectionState { get; init; }
}
