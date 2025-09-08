namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;

public interface IUpdateMyPupilsStateHandler
{
    Task HandleAsync(UpdateMyPupilsStateRequest request);
}
