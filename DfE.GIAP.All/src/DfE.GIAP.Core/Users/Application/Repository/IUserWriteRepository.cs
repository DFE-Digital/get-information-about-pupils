using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Application.Repository;
public interface IUserWriteRepository
{
    Task SaveMyPupilsAsync(
        UserId userId,
        IEnumerable<UniquePupilNumber> updatedPupilIds);
}
