namespace DfE.GIAP.Web.Features.MyPupils.Services.DeletePupils;

public interface IDeleteMyPupilsPresentationService
{
    Task DeletePupilsAsync(
        string userId,
        IEnumerable<string> selectedPupils);
}
