using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Repository.UserAggregate;
using DfE.GIAP.Core.MyPupils.Application.Services;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserAggregateWriteRepository _userAggregateWriteRepository;
    private readonly IAggregatePupilsForMyPupilsApplicationService _aggregatePupilsForMyPupilsService;

    public DeletePupilsFromMyPupilsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserAggregateWriteRepository userAggregateWriteRepository,
        IAggregatePupilsForMyPupilsApplicationService aggregatePupilsForMyPupilsService)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _userAggregateWriteRepository = userAggregateWriteRepository;
        _aggregatePupilsForMyPupilsService = aggregatePupilsForMyPupilsService;
    }

    public async Task HandleRequestAsync(
        DeletePupilsFromMyPupilsRequest request)
    {
        IEnumerable<UniquePupilNumber> parsedPupilIdentifiers = request.PupilIdentifiers.ToUniquePupilNumbers();

        UserId userId = new(request.UserId);

        User.Application.Repository.UserReadRepository.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        IEnumerable<Pupil> pupils = await _aggregatePupilsForMyPupilsService.GetPupilsAsync(parsedPupilIdentifiers);

        UserAggregateRoot userAggregate = new(user.UserId, pupils);

        userAggregate.RemovePupils(parsedPupilIdentifiers);

        await _userAggregateWriteRepository.SaveAsync(userAggregate);
    }
}
