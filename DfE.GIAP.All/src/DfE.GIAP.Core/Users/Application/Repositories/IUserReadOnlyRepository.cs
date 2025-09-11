namespace DfE.GIAP.Core.Users.Application.Repositories;

public interface IUserReadOnlyRepository
{
    Task<User> GetUserByIdAsync(UserId id, CancellationToken ctx = default);
    Task<User?> GetUserByIdIfExistsAsync(UserId id, CancellationToken ctx = default);
}
