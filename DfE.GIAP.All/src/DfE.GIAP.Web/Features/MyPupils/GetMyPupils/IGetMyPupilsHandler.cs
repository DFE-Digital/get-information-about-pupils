namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupils;
public interface IGetMyPupilsHandler
{
    Task<MyPupilsResponse> GetPupilsAsync(MyPupilsRequest request);
}
