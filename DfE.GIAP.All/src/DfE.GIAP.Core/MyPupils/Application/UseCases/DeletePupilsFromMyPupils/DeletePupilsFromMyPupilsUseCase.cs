using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repository.UserAggregate;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserAggregateWriteRepository _userAggregateWriteRepository;
    private readonly IAggregatePupilsForMyPupilsDomainService _aggregatePupilsForMyPupilsDomainService;

    public DeletePupilsFromMyPupilsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserAggregateWriteRepository userAggregateWriteRepository,
        IAggregatePupilsForMyPupilsDomainService aggregatePupilsForMyPupilsDomainService)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _userAggregateWriteRepository = userAggregateWriteRepository;
        _aggregatePupilsForMyPupilsDomainService = aggregatePupilsForMyPupilsDomainService;
    }

    public async Task HandleRequestAsync(
        DeletePupilsFromMyPupilsRequest request)
    {
        IEnumerable<PupilId> parsedPupilIdentifiers = request.PupilIdentifiers.Select(t =>
        {
            if (!Guid.TryParse(t, out Guid parsedIdentifier))
            {
                throw new ArgumentException($"Invalid pupil in list identifier {t}");
            }
            return new PupilId(parsedIdentifier);
        });

        UserId userId = new(request.AuthorisationContext.UserId);

        User.Application.Repository.UserReadRepository.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        UserAggregateRoot userAggregate = new(
            user.UserId,
            user.PupilIds,
            _aggregatePupilsForMyPupilsDomainService);

        userAggregate.RemovePupils(parsedPupilIdentifiers);

        await _userAggregateWriteRepository.SaveAsync(userAggregate);
    }
}
