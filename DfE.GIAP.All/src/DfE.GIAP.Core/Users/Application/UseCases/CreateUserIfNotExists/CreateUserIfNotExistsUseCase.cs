using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;

/// <summary>
/// Handles the creation of a user if the user does not already exist in the system.
/// </summary>
/// <remarks>This use case checks whether a user with the specified identifier already exists. If the user exists,
/// no action is taken.  If the user does not exist, a new user is created and persisted.</remarks>
public class CreateUserIfNotExistsUseCase : IUseCaseRequestOnly<CreateUserIfNotExistsRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteOnlyRepository _userWriteOnlyRepository;

    public CreateUserIfNotExistsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserWriteOnlyRepository userWriteOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(userReadOnlyRepository);
        ArgumentNullException.ThrowIfNull(userWriteOnlyRepository);
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteOnlyRepository = userWriteOnlyRepository;
    }


    /// <summary>
    /// Handles the creation of a user if the specified user does not already exist.
    /// </summary>
    /// <remarks>If a user with the specified ID already exists, no action is taken. Otherwise, a new user is
    /// created and saved with the current UTC timestamp.</remarks>
    /// <param name="request">The request containing the user ID to check and create if necessary. The <paramref name="request"/> parameter
    /// cannot be <see langword="null"/>.</param>
    /// <returns></returns>
    public async Task HandleRequestAsync(CreateUserIfNotExistsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);
        User? existingUser = await _userReadOnlyRepository.GetUserByIdIfExistsAsync(userId);

        if (existingUser is not null)
            return;

        User userToSave = new(userId, DateTime.UtcNow);
        await _userWriteOnlyRepository.UpsertUserAsync(userToSave);
    }
}
