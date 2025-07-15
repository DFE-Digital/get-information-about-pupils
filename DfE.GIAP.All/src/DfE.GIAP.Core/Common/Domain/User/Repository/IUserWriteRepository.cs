namespace DfE.GIAP.Core.Common.Domain.User.Repository;
internal interface IUserWriteRepository
{
    Task SaveAsync(UserAggregateRoot user);
}
