using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

public interface IGetPaginatedMyPupilsHandler
{
    Task<PupilDtos> HandleAsync(GetPaginatedMyPupilsRequest request);
}
