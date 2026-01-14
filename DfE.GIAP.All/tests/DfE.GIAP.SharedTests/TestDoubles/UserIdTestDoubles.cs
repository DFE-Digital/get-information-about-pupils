using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UserIdTestDoubles
{
    public static UserId Default() => new(Guid.NewGuid().ToString());
    public static UserId WithId(string id) => new(id);
}
