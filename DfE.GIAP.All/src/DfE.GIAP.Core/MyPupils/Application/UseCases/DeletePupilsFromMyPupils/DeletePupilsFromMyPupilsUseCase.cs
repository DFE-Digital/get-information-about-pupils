using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Application.Repository;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteRepository _userWriteRepository;

    public DeletePupilsFromMyPupilsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserWriteRepository userWriteRepository)
    {
        ArgumentNullException.ThrowIfNull(userWriteRepository);
        ArgumentNullException.ThrowIfNull(userWriteRepository);
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteRepository = userWriteRepository;
    }

    public async Task HandleRequestAsync(DeletePupilsFromMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        UserId userId = new(request.UserId);

        if (request.DeleteAll)
        {
            await _userWriteRepository.SaveMyPupilsAsync(userId, []);
            return;
        }

        User.Application.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        IEnumerable<string> userMyPupilUpnsBeforeDelete = user.UniquePupilNumbers.Select(t => t.Value);

        if (request.DeletePupilUpns.All(deleteUpn => !userMyPupilUpnsBeforeDelete.Contains(deleteUpn)))
        {
            throw new ArgumentException($"None of the pupil identifiers {string.Join(',', request.DeletePupilUpns)} are part of the User {userId.Value} MyPupils");
        }

        List<UniquePupilNumber> updatedMyPupilsAfterDelete =
            userMyPupilUpnsBeforeDelete
                .Where(upn => !request.DeletePupilUpns.Contains(upn))
                .ToUniquePupilNumbers()
                .ToList();

        await _userWriteRepository.SaveMyPupilsAsync(userId, updatedMyPupilsAfterDelete);
    }
}
