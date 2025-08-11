﻿using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class UserReadOnlyRepositoryTestDoubles
{
    internal static Mock<IUserReadOnlyRepository> Default() => new();

    internal static Mock<IUserReadOnlyRepository> MockForGetUserById(User stub)
    {
        Mock<IUserReadOnlyRepository> repoMock = Default();

        repoMock.Setup(t => t.GetUserByIdAsync(
                stub.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stub)
            .Verifiable();

        return repoMock;
    }
}
