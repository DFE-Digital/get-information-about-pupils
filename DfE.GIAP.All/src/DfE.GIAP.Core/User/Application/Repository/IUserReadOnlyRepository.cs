using DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils;
using DfE.GIAP.Core.User.Domain.Aggregate;

namespace DfE.GIAP.Core.User.Application.Repository;

public interface IUserReadOnlyRepository
{
    Task<User> GetByIdAsync(UserIdentifier id, IAuthorisationContext authorisationContext, CancellationToken ctx = default);
}
