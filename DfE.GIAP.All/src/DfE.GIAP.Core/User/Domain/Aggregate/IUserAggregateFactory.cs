using DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils;

namespace DfE.GIAP.Core.User.Domain.Aggregate;
public interface IUserAggregateFactory
{
    Task<UserAggregateRoot> Create(IAuthorisationContext authorisationContext);
}
