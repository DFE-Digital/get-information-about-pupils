namespace DfE.GIAP.Core.Users.Application.Repositories;

public interface IUserWriteOnlyRepository
{
    Task UpsertUserAsync(User user);
}
