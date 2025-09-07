using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public record MyPupilsState
{
    public MyPupilsState(MyPupilsPresentationState presentationState, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(presentationState);
        PresentationState = presentationState;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilsPresentationState PresentationState { get; init; }
    public MyPupilsPupilSelectionState SelectionState { get; init; }
}
