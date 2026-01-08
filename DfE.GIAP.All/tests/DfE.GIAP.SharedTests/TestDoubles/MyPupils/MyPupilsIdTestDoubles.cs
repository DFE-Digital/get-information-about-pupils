using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.MyPupils;
public static class MyPupilsIdTestDoubles
{
    public static MyPupilsId Default() => new(Guid.NewGuid().ToString());
    public static MyPupilsId Create(string id) => new(id);
}
