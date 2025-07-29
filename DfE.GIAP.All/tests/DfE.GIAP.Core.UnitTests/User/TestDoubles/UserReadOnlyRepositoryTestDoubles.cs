using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;

namespace DfE.GIAP.Core.UnitTests.User.TestDoubles;
internal static class UserReadOnlyRepositoryTestDoubles
{
    internal static IUserReadOnlyRepository Default() => CreateMock().Object;

    internal static Mock<IUserReadOnlyRepository> MockForGetUserById(Core.User.Application.Repository.UserReadRepository.User repositoryResponse)
    {
        Mock<IUserReadOnlyRepository> mock = CreateMock();

        mock.Setup(repository => repository.GetUserByIdAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResponse);

        return mock;

    }

    private static Mock<IUserReadOnlyRepository> CreateMock() => new();
}
