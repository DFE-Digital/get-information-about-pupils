using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;

/// <summary>
/// Handles the use case for updating the "Last Logged In" timestamp of a user.
/// </summary>
/// <remarks>This use case retrieves the user by their unique identifier, updates their "Last Logged In"
/// timestamp,  and persists the changes to the data store. It ensures that the provided request and repositories are
/// not null.</remarks>
public class UpdateLastLoggedInUseCase : IUseCaseRequestOnly<UpdateLastLoggedInRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteOnlyRepository _userWriteOnlyRepository;

    public UpdateLastLoggedInUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserWriteOnlyRepository userWriteOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(userReadOnlyRepository);
        ArgumentNullException.ThrowIfNull(userWriteOnlyRepository);
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteOnlyRepository = userWriteOnlyRepository;
    }

    /// <summary>
    /// Handles the update of a user's last logged-in timestamp.
    /// </summary>
    /// <param name="request">The request containing the user ID and the new last logged-in timestamp. Cannot be <see langword="null"/>.</param>
    /// <returns></returns>
    public async Task HandleRequestAsync(UpdateLastLoggedInRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);
        User existingUser = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        User userToUpdate = existingUser with { LastLoggedIn = request.LastLoggedIn };

        await _userWriteOnlyRepository.UpsertUserAsync(userToUpdate);
    }
}
