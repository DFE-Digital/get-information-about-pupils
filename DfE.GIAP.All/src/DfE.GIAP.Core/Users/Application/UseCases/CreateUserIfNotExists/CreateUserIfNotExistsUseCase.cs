using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
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

    public async Task HandleRequestAsync(CreateUserIfNotExistsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);
        User? existingUser = await _userReadOnlyRepository.GetUserByIdIfExistsAsync(userId);

        if (existingUser is not null)
            return;

        User userToSave = new(userId, [], DateTime.UtcNow);
        await _userWriteOnlyRepository.UpsertUserAsync(userToSave);
    }
}
