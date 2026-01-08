using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;

/// <summary>
/// Represents a request to create a user if the user does not already exist.
/// </summary>
/// <remarks>This request is typically used in scenarios where user creation is conditional on the user not being
/// present in the system.</remarks>
/// <param name="UserId">The unique identifier of the user to be created. This value must not be null or empty.</param>
public record CreateUserIfNotExistsRequest(string UserId) : IUseCaseRequest;
