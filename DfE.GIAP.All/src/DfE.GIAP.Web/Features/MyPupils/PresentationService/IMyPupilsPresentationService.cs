namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public interface IMyPupilsPresentationService
{
    Task<MyPupilsPresentationResponse> GetPupils(
        string userId,
        int pageNumber,
        string sortField,
        string sortDirection);
    Task<IEnumerable<string>> GetSelectedPupilUniquePupilNumbers(string userId);
}
