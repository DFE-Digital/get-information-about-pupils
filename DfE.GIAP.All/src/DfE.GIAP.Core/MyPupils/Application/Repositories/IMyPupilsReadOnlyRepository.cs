using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;
public interface IMyPupilsReadOnlyRepository
{
    Task<Domain.AggregateRoot.MyPupils> GetMyPupils(UserId userId, CancellationToken ctx = default);
    Task<Domain.AggregateRoot.MyPupils?> GetMyPupilsOrDefaultAsync(UserId userId, CancellationToken ctx = default);
}
