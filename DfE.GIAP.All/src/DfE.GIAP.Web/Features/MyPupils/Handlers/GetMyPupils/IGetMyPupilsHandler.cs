using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public interface IGetMyPupilsHandler
{
    Task<PupilsViewModel> HandleAsync(GetMyPupilsRequest request);
}
