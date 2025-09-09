namespace DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState;

public interface IUpdateMyPupilsStateHandler
{
    Task HandleAsync(UpdateMyPupilsStateRequest request);
}
