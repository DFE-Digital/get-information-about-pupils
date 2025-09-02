namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

public interface IGetPaginatedMyPupilsHandler
{
    Task<PaginatedMyPupilsResponse> HandleAsync(GetPaginatedMyPupilsRequest request);
}
