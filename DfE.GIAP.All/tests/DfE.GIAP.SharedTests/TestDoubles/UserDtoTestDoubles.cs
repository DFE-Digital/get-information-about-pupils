using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class UserDtoTestDoubles
{
    public static UserDto Default() => new()
    {
        id = Guid.NewGuid().ToString()
    };

    public static UserDto WithId(string id) => WithId(new UserId(id));
    public static UserDto WithId(UserId id) => WithPupils(id, []);
    public static UserDto WithPupils(string id, IEnumerable<UniquePupilNumber> upns) => WithPupils(new UserId(id), upns);

    public static UserDto WithPupils(
        UserId id,
        IEnumerable<UniquePupilNumber> upns)
    {

        IEnumerable<MyPupilsItemDto>? myPupils = upns.Select((upn) => new MyPupilsItemDto()
        {
            UPN = upn.Value
        });

        return new()
        {
            id = id.Value,
            MyPupils = new()
            {
                Pupils = myPupils
            }
        };
    }
}
