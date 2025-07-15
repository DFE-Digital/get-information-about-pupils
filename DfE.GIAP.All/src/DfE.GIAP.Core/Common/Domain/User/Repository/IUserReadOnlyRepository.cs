using DfE.GIAP.Core.MyPupils.Domain;

namespace DfE.GIAP.Core.Common.Domain.User.Repository;

public interface IUserReadOnlyRepository
{
    Task<UserAggregateRoot> GetByIdAsync(UserIdentifier id, MyPupilsAuthorisationContext myPupilsAuthorisationContext, CancellationToken ctx = default);
}
