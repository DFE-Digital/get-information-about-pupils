using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repository;
using DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.User.Application.Repository;

namespace DfE.GIAP.Core.User.Domain.Aggregate;
internal sealed class UserAggregateRootFactory : IUserAggregateFactory
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IMyPupilReadOnlyRepository _myPupilReadOnlyRepository;
    private readonly IMapper<IAuthorisationContext, MyPupilsAuthorisationContext> _mapApplicationAuthorisationToMyPupilsAuthorisationContext;

    public UserAggregateRootFactory(
        IUserReadOnlyRepository readOnlyRepository,
        IMyPupilReadOnlyRepository myPupilReadOnlyRepository,
        IMapper<IAuthorisationContext, MyPupilsAuthorisationContext> mapApplicationAuthorisationToMyPupilsAuthorisationContext)
    {
        _userReadOnlyRepository = readOnlyRepository;
        _myPupilReadOnlyRepository = myPupilReadOnlyRepository;
        _mapApplicationAuthorisationToMyPupilsAuthorisationContext = mapApplicationAuthorisationToMyPupilsAuthorisationContext;
    }

    public async Task<UserAggregateRoot> Create(IAuthorisationContext authorisationContext)
    {
        UserIdentifier identfiier = new(authorisationContext.UserId);

        Application.Repository.User user = await _userReadOnlyRepository.GetByIdAsync(identfiier, authorisationContext);

        MyPupilsAuthorisationContext myPupilsAuthorisationContext = _mapApplicationAuthorisationToMyPupilsAuthorisationContext.Map(authorisationContext);

        IEnumerable<MyPupil> myPupils = await _myPupilReadOnlyRepository.GetMyPupilsById(user.MyPupilsIds, myPupilsAuthorisationContext);

        return new UserAggregateRoot(identfiier, myPupils);
    }
}
