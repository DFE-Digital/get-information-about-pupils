using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;

public interface IUserReadOnlyRepository
{
    Task<User> GetUserByIdAsync(UserId id, IAuthorisationContext authorisationContext, CancellationToken ctx = default);
}
