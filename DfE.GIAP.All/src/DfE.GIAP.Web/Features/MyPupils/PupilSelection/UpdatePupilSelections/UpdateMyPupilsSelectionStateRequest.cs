using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public record UpdateMyPupilsSelectionStateRequest
{
    public UpdateMyPupilsSelectionStateRequest(MyPupilsPupilSelectionsRequestDto selectionsDto, MyPupilsPupilSelectionState currentState)
    {
        ArgumentNullException.ThrowIfNull(selectionsDto);
        UpdateRequest = selectionsDto;

        ArgumentNullException.ThrowIfNull(currentState);
        State = currentState;
    }

    public MyPupilsPupilSelectionsRequestDto UpdateRequest { get; init; }
    public MyPupilsPupilSelectionState State { get; init; }
}
