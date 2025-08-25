namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilSelectionState.Provider;

public interface IPupilSelectionStateProvider
{
    PupilsSelectionState GetState();
    void UpdateState(PupilsSelectionState state);
}
