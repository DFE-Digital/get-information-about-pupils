namespace DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;

public interface IGetPaginatedMyPupilsHandler
{
    Task<PaginatedMyPupilsResponse> HandleAsync(GetPaginatedMyPupilsRequest request);
}
