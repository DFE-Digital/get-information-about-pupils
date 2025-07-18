using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.User.TestDoubles;
internal static class UserReadOnlyRepositoryTestDoubles
{
    internal static IUserReadOnlyRepository Default() => CreateMock().Object;

    internal static Mock<IUserReadOnlyRepository> MockForGetUserById(Core.User.Application.Repository.User repositoryResponse)
    {
        Mock<IUserReadOnlyRepository> mock = CreateMock();

        mock.Setup(repository => repository.GetUserByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<IAuthorisationContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResponse);

        return mock;

    }

    private static Mock<IUserReadOnlyRepository> CreateMock() => new();
}
