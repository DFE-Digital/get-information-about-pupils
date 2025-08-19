namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Provider;

public interface IPupilSelectionStateProvider
{
    PupilsSelectionState GetState();
    void UpdateState(PupilsSelectionState state);
}
