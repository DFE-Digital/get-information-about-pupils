namespace DfE.GIAP.Core.User.Application.Repository;
public interface IUserReadOnlyRepository
{
    Task<User> GetUserByIdAsync(UserId id, CancellationToken ctx = default);
}
