namespace DfE.GIAP.Core.Users.Application.Repository;

public interface IUserReadOnlyRepository
{
    Task<User> GetUserByIdAsync(UserId id, CancellationToken ctx = default);
}
