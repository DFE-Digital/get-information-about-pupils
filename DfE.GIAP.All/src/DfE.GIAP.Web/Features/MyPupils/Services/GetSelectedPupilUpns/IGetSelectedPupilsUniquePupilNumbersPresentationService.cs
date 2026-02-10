namespace DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;
public interface IGetSelectedPupilsUniquePupilNumbersPresentationService
{
    Task<IEnumerable<string>> GetSelectedPupilsAsync(string userId);
}
