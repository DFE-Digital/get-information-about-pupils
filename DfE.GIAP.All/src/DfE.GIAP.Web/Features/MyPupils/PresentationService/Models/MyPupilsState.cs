using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record MyPupilsState
{
    public MyPupilsState(MyPupilsPresentationQueryModel query, MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(query);
        PresentationState = query;

        ArgumentNullException.ThrowIfNull(selectionState);
        SelectionState = selectionState;
    }

    public MyPupilsPresentationQueryModel PresentationState { get; init; }
    public MyPupilsPupilSelectionState SelectionState { get; init; }

    public static MyPupilsState Create(
        MyPupilsPresentationQueryModel query, MyPupilsPupilSelectionState selectionState) => new(query, selectionState);
}
