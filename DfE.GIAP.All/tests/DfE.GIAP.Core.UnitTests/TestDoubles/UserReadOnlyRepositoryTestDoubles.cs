using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class UserReadOnlyRepositoryTestDoubles
{
    internal static Mock<IUserReadOnlyRepository> Default() => new();

    internal static Mock<IUserReadOnlyRepository> MockForGetUserById(User.Application.User stub, UserId? userId = null)
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
