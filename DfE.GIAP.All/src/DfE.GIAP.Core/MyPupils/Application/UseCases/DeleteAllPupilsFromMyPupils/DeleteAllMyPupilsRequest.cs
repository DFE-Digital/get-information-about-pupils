using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
public record DeleteAllMyPupilsRequest(string UserId) : IUseCaseRequest;
