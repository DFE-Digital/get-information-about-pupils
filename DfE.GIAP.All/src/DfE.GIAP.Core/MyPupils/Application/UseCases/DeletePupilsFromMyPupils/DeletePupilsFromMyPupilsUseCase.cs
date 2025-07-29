using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;
using DfE.GIAP.Core.User.Application.Repository.UserWriteRepository;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;

internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteRepository _userWriteRepository;

    public DeletePupilsFromMyPupilsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserWriteRepository userWriteRepository)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteRepository = userWriteRepository;
    }

    public async Task HandleRequestAsync(DeletePupilsFromMyPupilsRequest request)
    {
        int upnLimit = 4000;
        UserId userId = new(request.UserId);

        User.Application.Repository.UserReadRepository.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        IEnumerable<string> userMyPupilsUpns = user.UniquePupilNumbers.Select(t => t.Value);

        if (!request.PupilIdentifiers.All(deletePupilUpn => userMyPupilsUpns.Contains(deletePupilUpn)))
        {
            throw new ArgumentException($"None of the pupil identifiers {string.Join(',', request.PupilIdentifiers)} are part of the User {userId.Value} MyPupils");
        }

        List<UniquePupilNumber> updatedPupilIds =
            user.UniquePupilNumbers
                .Where(p => !request.PupilIdentifiers.ToUniquePupilNumbers().Contains(p))
                .Select(p => p)
                .Distinct()
                .Take(upnLimit)
                .ToList();

        await _userWriteRepository.SaveMyPupilsAsync(userId, updatedPupilIds);
    }
}
