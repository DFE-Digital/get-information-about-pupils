using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UserTestDoubles
{
    public static User Default()
    {
        UserId userId = UserIdTestDoubles.Default();
        User user = new(userId, DateTime.UtcNow);
        return user;
    }
}
