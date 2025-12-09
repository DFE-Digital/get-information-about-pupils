
namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public interface IMyPupilsPresentationService
{
    Task<MyPupilsPresentationResponse> GetPupils(string userId);
    Task<IEnumerable<string>> GetSelectedPupilUniquePupilNumbers(string userId);
}
