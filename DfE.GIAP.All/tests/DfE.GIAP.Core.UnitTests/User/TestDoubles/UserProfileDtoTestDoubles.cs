using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Infrastructure;

namespace DfE.GIAP.Core.UnitTests.User.TestDoubles;
internal static class UserProfileDtoTestDoubles
{
    internal static UserProfileDto Default() => new()
    {
        UserId = "user"
    };

    internal static UserProfileDto WithId(UserId id) => WithId(id.Value);

    internal static UserProfileDto WithId(string? id) => new()
    {
        UserId = id
    };

    internal static UserProfileDto WithPupils(
        UserId id,
        string[]? pupils,
        IEnumerable<PupilItemDto>? myPupils) => new()
        {
            UserId = id.Value,
            PupilList = pupils!,
            MyPupilList = myPupils!,
        };
}
