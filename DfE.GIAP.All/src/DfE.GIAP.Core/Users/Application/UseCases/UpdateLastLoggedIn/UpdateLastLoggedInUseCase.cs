using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;

public class UpdateLastLoggedInUseCase : IUseCaseRequestOnly<UpdateLastLoggedInRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteOnlyRepository _userWriteOnlyRepository;

    public UpdateLastLoggedInUseCase(IUserReadOnlyRepository userReadOnlyRepository, IUserWriteOnlyRepository userWriteOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(userReadOnlyRepository);
        ArgumentNullException.ThrowIfNull(userWriteOnlyRepository);
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteOnlyRepository = userWriteOnlyRepository;
    }

    public async Task HandleRequestAsync(UpdateLastLoggedInRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);
        User existingUser = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        User userToUpdate = existingUser with { LastLoggedIn = request.LastLoggedIn };

        await _userWriteOnlyRepository.UpsertUserAsync(userToUpdate);
    }
}
