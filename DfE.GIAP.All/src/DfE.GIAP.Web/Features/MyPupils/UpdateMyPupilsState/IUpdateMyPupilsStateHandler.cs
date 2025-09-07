namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;

public interface IUpdateMyPupilsStateHandler
{
    Task Handle(UpdateMyPupilsStateRequest request);
}
