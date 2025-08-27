using DfE.GIAP.Web.Features.MyPupils.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public interface IGetMyPupilsHandler
{
    Task<IEnumerable<PupilViewModel>> HandleAsync(GetMyPupilsRequest request);
}
