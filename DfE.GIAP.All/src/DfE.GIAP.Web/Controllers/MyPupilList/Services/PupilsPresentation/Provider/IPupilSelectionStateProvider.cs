using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Provider;

public interface IPupilSelectionStateProvider
{
    PupilsSelectionState GetState();
    void UpdateState(PupilsSelectionState state);
}
