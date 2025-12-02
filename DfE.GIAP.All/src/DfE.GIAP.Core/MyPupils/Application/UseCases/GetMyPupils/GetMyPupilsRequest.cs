using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record GetMyPupilsRequest(string MyPupilsId) : IUseCaseRequest<GetMyPupilsResponse>;
