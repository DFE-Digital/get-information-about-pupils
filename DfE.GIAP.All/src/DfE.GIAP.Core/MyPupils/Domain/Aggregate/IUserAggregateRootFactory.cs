using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;

namespace DfE.GIAP.Core.MyPupils.Domain.Aggregate;
public interface IUserAggregateRootFactory
{
    Task<UserAggregateRoot> CreateAsync(IAuthorisationContext authorisationContext);
}
