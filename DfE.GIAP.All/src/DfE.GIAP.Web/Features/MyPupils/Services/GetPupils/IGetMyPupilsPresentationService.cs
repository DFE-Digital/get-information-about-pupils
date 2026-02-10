using DfE.GIAP.Web.Features.MyPupils.Controllers;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;
public interface IGetMyPupilsPresentationService
{
    Task<MyPupilsPresentationResponse> GetPupilsAsync(
        string userId,
        MyPupilsQueryRequestDto query);
}
