using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

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

    public static User WithEmptyUpns() => WithUpns([]);

    public static User WithUpns(IEnumerable<UniquePupilNumber> upns)
    {
        UserId userId = UserIdTestDoubles.Default();
        User user = new(userId, upns);
        return user;
    }
}
