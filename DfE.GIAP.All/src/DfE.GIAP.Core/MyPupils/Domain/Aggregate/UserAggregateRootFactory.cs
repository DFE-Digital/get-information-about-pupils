using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
internal sealed class UserAggregateRootFactory : IUserAggregateRootFactory
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IAggregatePupilsForMyPupilsDomainService _aggregatePupilsForMyPupilDomainService;
    private readonly IMapper<IAuthorisationContext, PupilAuthorisationContext> _mapApplicationAuthorisationToMyPupilsAuthorisationContext;

    public UserAggregateRootFactory(
        IUserReadOnlyRepository userReadOnlyRepository,
        IAggregatePupilsForMyPupilsDomainService aggregatePupilsForMyPupilDomainService,
        IMapper<IAuthorisationContext, PupilAuthorisationContext> mapApplicationAuthorisationToMyPupilsAuthorisationContext)
    {
        ArgumentNullException.ThrowIfNull(userReadOnlyRepository);
        ArgumentNullException.ThrowIfNull(aggregatePupilsForMyPupilDomainService);
        ArgumentNullException.ThrowIfNull(mapApplicationAuthorisationToMyPupilsAuthorisationContext);
        _userReadOnlyRepository = userReadOnlyRepository;
        _aggregatePupilsForMyPupilDomainService = aggregatePupilsForMyPupilDomainService;
        _mapApplicationAuthorisationToMyPupilsAuthorisationContext = mapApplicationAuthorisationToMyPupilsAuthorisationContext;
    }

    public async Task<UserAggregateRoot> CreateAsync(IAuthorisationContext authorisationContext)
    {
        UserId identfiier = new(authorisationContext.UserId);

        User.Application.Repository.User user = await _userReadOnlyRepository.GetUserByIdAsync(identfiier, authorisationContext);
        PupilAuthorisationContext myPupilsAuthorisationContext = _mapApplicationAuthorisationToMyPupilsAuthorisationContext.Map(authorisationContext);

        IEnumerable<Pupil> myPupils = await _aggregatePupilsForMyPupilDomainService.GetPupilsAsync(user.PupilIdentifiers, myPupilsAuthorisationContext);

        return new UserAggregateRoot(identfiier, myPupils);
    }
}
