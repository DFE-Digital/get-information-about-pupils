namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.UpdatePresentationState;

public interface IUpdateMyPupilsStateCommandHandler
{
    void Handle(MyPupilsFormStateRequestDto request);
}

