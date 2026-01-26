using DfE.GIAP.Web.Features.MyPupils.Controllers;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
public interface IGetMyPupilsPresentationService
{
    Task<MyPupilsPresentationResponse> GetPupilsAsync(
        string userId,
        MyPupilsQueryRequestDto query);
}
