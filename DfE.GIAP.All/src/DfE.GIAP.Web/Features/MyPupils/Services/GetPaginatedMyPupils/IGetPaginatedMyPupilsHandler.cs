namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;

public interface IGetPaginatedMyPupilsHandler
{
    Task<PaginatedMyPupilsResponse> HandleAsync(GetPaginatedMyPupilsRequest request);
}
