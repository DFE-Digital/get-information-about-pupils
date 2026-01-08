using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UserTestDoubles
{
    public static User Default()
    {
        UserId userId = UserIdTestDoubles.Default();
        User user = new(userId, DateTime.UtcNow);
        return user;
    }

    public static User WithId(UserId id) => new(id, DateTime.UtcNow);
}
