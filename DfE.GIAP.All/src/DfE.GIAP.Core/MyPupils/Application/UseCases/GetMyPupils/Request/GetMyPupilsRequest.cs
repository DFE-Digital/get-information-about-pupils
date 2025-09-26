using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public record GetMyPupilsRequest(string UserId) : IUseCaseRequest<GetMyPupilsResponse>;
