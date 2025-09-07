using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils;

public record PaginatedMyPupilsResponse(MyPupilDtos Pupils);
