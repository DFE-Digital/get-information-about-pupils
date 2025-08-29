using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider;

public interface IMyPupilsPupilSelectionStateProvider
{
    MyPupilsPupilSelectionState GetState();
    void SetState(MyPupilsPupilSelectionState state);
}
