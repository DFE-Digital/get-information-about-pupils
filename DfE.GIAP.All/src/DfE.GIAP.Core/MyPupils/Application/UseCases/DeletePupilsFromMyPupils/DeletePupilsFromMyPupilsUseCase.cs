using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IMyPupilsReadOnlyRepository _myPupilsReadOnlyRepository;
    private readonly IMyPupilsWriteOnlyRepository _myPupilsWriteOnlyRepository;

    public DeletePupilsFromMyPupilsUseCase(
        IMyPupilsReadOnlyRepository myPupilsReadOnlyRepository,
        IMyPupilsWriteOnlyRepository myPupilsWriteOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(myPupilsReadOnlyRepository);
        _myPupilsReadOnlyRepository = myPupilsReadOnlyRepository;

        ArgumentNullException.ThrowIfNull(myPupilsWriteOnlyRepository);
        _myPupilsWriteOnlyRepository = myPupilsWriteOnlyRepository;
    }

    public async Task HandleRequestAsync(DeletePupilsFromMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        UserId userId = new(request.UserId);

        if (request.DeleteAll)
        {
            await _myPupilsWriteOnlyRepository.SaveMyPupilsAsync(userId, UniquePupilNumbers.Create(uniquePupilNumbers: []));
            return;
        }

        Repositories.MyPupils myPupils = await _myPupilsReadOnlyRepository.GetMyPupils(userId);

        IEnumerable<string> userMyPupilUpnsBeforeDelete = myPupils.Pupils.AsValues();

        if (request.DeletePupilUpns.All(deleteUpn => !userMyPupilUpnsBeforeDelete.Contains(deleteUpn)))
        {
            throw new ArgumentException($"None of the pupil identifiers {string.Join(',', request.DeletePupilUpns)} are part of the User {userId.Value} MyPupils");
        }

        List<UniquePupilNumber> updatedMyPupilsAfterDelete =
            userMyPupilUpnsBeforeDelete
                .Where(upn => !request.DeletePupilUpns.Contains(upn))
                .ToUniquePupilNumbers()
                .ToList();

        await _myPupilsWriteOnlyRepository.SaveMyPupilsAsync(userId, myPupils.Pupils);
    }
}
