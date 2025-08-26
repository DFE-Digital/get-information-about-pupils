namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;

public interface IPupilSelectionStateProvider
{
    IMyPupilsPupilSelectionState GetState();
    void SetState(IMyPupilsPupilSelectionState state);
}
