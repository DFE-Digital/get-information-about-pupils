using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class UserReadOnlyRepositoryTestDoubles
{
    internal static Mock<IUserReadOnlyRepository> Default() => new();

    internal static Mock<IUserReadOnlyRepository> MockForGetUserById(User stub, UserId? userId = null)
    {
        Mock<IUserReadOnlyRepository> repoMock = Default();

        UserId matchUserId = userId is null ? It.IsAny<UserId>() : userId;

        repoMock.Setup(t => t.GetUserByIdAsync(
                matchUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stub)
            .Verifiable();

        return repoMock;
    }
}
