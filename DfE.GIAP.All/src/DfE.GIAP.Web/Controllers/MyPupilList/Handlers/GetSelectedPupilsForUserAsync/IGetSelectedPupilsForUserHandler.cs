namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetSelectedPupilsForUserAsync;

public interface IGetSelectedPupilsForUserHandler
{
    Task<IEnumerable<string>> GetSelectedPupilsForUserAsync(string userId);
}
