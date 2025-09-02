using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
public record CreateUserIfNotExistsRequest(string UserId) : IUseCaseRequest;
