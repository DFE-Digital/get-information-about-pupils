using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.Users.Application.Repositories;

public interface IUserWriteOnlyRepository
{
    Task SaveMyPupilsAsync(UserId userId, IEnumerable<UniquePupilNumber> updatedPupilIds);
    Task UpsertUserAsync(User user);
}
