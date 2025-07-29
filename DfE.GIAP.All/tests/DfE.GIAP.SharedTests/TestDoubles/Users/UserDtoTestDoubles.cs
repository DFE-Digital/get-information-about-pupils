using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Infrastructure.Repository;

namespace DfE.GIAP.SharedTests.TestDoubles.Users;
public static class UserDtoTestDoubles
{
    public static UserDto Default() => new()
    {
        id = "user"
    };

    public static UserDto WithId(UserId id) => WithId(id.Value);

    public static UserDto WithId(string? id) => new()
    {
        id = id
    };

    // Uses new structure. Can deprecate ^ once confirmed
    public static UserDto WithPupils(
        UserId id,
        IEnumerable<MyPupilItemDto>? myPupils) => new()
        {
            id = id.Value,
            MyPupils = myPupils!,
        };
}
