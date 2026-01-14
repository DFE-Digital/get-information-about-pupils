using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public record UpdateMyPupilsSelectionStateRequest
{
    public UpdateMyPupilsSelectionStateRequest(MyPupilsFormStateRequestDto updateRequest, MyPupilsPupilSelectionState currentState)
    {
        ArgumentNullException.ThrowIfNull(updateRequest);
        UpdateRequest = updateRequest;

        ArgumentNullException.ThrowIfNull(currentState);
        State = currentState;
    }

    public MyPupilsFormStateRequestDto UpdateRequest { get; init; }
    public MyPupilsPupilSelectionState State { get; init; }
}
