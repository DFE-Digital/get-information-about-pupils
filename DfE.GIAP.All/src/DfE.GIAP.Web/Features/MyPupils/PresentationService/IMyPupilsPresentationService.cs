using DfE.GIAP.Web.Features.MyPupils.Controllers;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public interface IMyPupilsPresentationService
{
    Task DeletePupils(
        string userId,
        IEnumerable<string> selectedPupilUpns);

    Task<MyPupilsPresentationResponse> GetPupils(
        string userId,
        MyPupilsQueryRequestDto query);

    Task<IEnumerable<string>> GetSelectedPupilUniquePupilNumbers(string userId);
}
