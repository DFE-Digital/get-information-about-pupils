using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler;

namespace DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

public interface IGetMyPupilsHandler
{
    Task<MyPupilsResponse> GetPupilsAsync(MyPupilsRequest request);
}
