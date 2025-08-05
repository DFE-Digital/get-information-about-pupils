using DfE.GIAP.Core.User.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class UserWriteRepositoryTestDoubles
{
    internal static Mock<IUserWriteRepository> Default() => new();
}
