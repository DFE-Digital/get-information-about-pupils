using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public static class MyPupilsIdTestDoubles
{
    public static MyPupilsId Default() => new(userId: UserIdTestDoubles.Default());
    public static MyPupilsId Create(string id) => new(UserIdTestDoubles.WithId(id));
}