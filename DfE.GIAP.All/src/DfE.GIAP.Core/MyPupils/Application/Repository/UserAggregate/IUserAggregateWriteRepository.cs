using DfE.GIAP.Core.MyPupils.Domain.Aggregate;

namespace DfE.GIAP.Core.MyPupils.Application.Repository.UserAggregate;
internal interface IUserAggregateWriteRepository
{
    Task SaveAsync(UserAggregateRoot userAggregate);
}

