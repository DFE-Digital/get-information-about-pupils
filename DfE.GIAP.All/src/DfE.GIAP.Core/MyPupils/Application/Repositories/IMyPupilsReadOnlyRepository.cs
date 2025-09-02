using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;
public interface IMyPupilsReadOnlyRepository
{
    Task<MyPupils> GetMyPupils(UserId userId, CancellationToken ctx = default);
}
