namespace DfE.GIAP.Web.Controllers.MyPupilList.PresentationState.Provider;

public interface IMyPupilsPresentationStateProvider
{
    MyPupilsPresentationState Get();
    void Set(MyPupilsPresentationState options);
}
