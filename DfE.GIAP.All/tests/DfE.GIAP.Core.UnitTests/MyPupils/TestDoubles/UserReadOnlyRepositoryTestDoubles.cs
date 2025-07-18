using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class UserReadOnlyRepositoryTestDoubles
{
    internal static IUserReadOnlyRepository Default() => CreateMock().Object;

    internal static Mock<IUserReadOnlyRepository> MockForGetUserById(User repositoryResponse)
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
