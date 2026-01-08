using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.SharedTests.Features.MyPupils.Domain;
public static class MyPupilsIdTestDoubles
{
    public static MyPupilsId Default() => new(userId: UserIdTestDoubles.Default());
    public static MyPupilsId Create(string id) => new(UserIdTestDoubles.WithId(id));
}
