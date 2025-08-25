namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;

public interface IPupilSelectionStateProvider
{
    IPupilsSelectionState GetState();
    void SetState(IPupilsSelectionState state);
}
