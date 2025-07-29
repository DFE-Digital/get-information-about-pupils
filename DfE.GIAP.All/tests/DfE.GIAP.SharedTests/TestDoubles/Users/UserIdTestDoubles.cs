using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;

namespace DfE.GIAP.SharedTests.TestDoubles.Users;
public static class UserIdTestDoubles
{
    public static UserId Default() => new(Guid.NewGuid().ToString());
    public static UserId WithId(string id) => new(id);
}
