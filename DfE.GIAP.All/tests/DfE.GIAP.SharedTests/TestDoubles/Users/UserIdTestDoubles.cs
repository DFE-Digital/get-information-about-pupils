using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.Users;
public static class UserIdTestDoubles
{
    public static UserId Default() => new(Guid.NewGuid().ToString());
}
