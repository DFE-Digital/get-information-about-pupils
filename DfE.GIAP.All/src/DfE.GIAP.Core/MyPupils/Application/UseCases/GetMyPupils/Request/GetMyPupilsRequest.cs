using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public record GetMyPupilsRequest(string UserId, MyPupilsQueryOptions? Options = null) : IUseCaseRequest<GetMyPupilsResponse>;
