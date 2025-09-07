using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils;

public interface IGetPaginatedMyPupilsHandler
{
    Task<PaginatedMyPupilsResponse> HandleAsync(GetPaginatedMyPupilsRequest request);
}
