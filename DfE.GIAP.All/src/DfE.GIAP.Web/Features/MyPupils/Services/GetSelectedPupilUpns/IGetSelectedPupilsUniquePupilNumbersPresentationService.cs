namespace DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilIdentifiers;
public interface IGetSelectedPupilsUniquePupilNumbersPresentationService
{
    Task<IEnumerable<string>> GetSelectedPupilsAsync(string userId);
}
