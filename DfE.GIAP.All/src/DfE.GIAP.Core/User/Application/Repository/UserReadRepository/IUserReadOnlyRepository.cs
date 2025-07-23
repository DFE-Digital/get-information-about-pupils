using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.User.Application.Repository.UserReadRepository;

public interface IUserReadOnlyRepository
{
    Task<User> GetUserByIdAsync(UserId id, CancellationToken ctx = default);
}
