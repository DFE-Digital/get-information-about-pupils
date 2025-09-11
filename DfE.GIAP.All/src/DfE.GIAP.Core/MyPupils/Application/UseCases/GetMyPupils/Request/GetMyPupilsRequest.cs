using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public record GetMyPupilsRequest(UserId UserId) : IUseCaseRequest<GetMyPupilsResponse>;
