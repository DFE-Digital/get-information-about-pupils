using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UserTestDoubles
{
    public static User Default()
    {
        UserId userId = UserIdTestDoubles.Default();
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 10);
        User user = new(userId, upns);
        return user;
    }

    public static User WithEmptyUpns()
    {
        UserId userId = UserIdTestDoubles.Default();
        User user = new(userId, []);
        return user;
    }
}
