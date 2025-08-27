using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationState.Provider;

public interface IMyPupilsPresentationStateProvider
{
    MyPupilsPresentationState GetState();
    void SetState(MyPupilsPresentationState state);
}
