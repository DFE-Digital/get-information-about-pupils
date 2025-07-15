using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;

namespace DfE.GIAP.Core.User.Domain.Aggregate;
public interface IUserAggregateFactory
{
    Task<UserAggregateRoot> Create(IAuthorisationContext authorisationContext);
}
