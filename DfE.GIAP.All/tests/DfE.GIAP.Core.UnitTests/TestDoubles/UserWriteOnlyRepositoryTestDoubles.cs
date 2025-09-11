using DfE.GIAP.Core.Users.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class UserWriteOnlyRepositoryTestDoubles
{
    internal static Mock<IUserWriteOnlyRepository> Default() => new();
}
