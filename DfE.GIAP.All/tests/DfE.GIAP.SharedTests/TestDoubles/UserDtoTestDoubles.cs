using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UserDtoTestDoubles
{
    public static UserDto Default() => new()
    {
        id = "user"
    };

    public static UserDto WithId(UserId id) => WithPupils(id, []);

    public static UserDto WithPupils(
        UserId id,
        IEnumerable<MyPupilsItemDto>? myPupils) => new()
        {
            id = id.Value,
            MyPupils = new()
            {
                Pupils = myPupils
            },
        };
}
