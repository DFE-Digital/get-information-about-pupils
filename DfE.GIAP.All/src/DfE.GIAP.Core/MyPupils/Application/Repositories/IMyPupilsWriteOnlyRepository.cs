using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;

public interface IMyPupilsWriteOnlyRepository
{
    Task SaveMyPupilsAsync(UserId userId, UniquePupilNumbers updatedMyPupils, CancellationToken ctx = default);
}
