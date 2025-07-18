using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record GetMyPupilsRequest(IAuthorisationContext AuthContext) : IUseCaseRequest<GetMyPupilsResponse>;
