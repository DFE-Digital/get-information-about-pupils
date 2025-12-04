namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler;
public interface IGetMyPupilsHandler
{
    Task<MyPupilsResponse> GetPupilsAsync(MyPupilsRequest request);
}
