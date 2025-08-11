using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;

public sealed class DeletePupilsFromMyPupilsUseCaseTests
{
    [Fact]
    public async Task HandleRequestAsync_WhenDeleteAllIsTrue_SavesEmptyList()
    {
        // Arrange
        User user = UserTestDoubles.Default();
        Mock<IUserReadOnlyRepository> mockReadRepository = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user);
        Mock<IUserWriteOnlyRepository> writeRepoDouble = UserWriteRepositoryTestDoubles.Default();
        DeletePupilsFromMyPupilsUseCase useCase = new(mockReadRepository.Object, writeRepoDouble.Object);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: user.UserId.Value,
            DeleteAll: true,
            DeletePupilUpns: ["UPN1", "UPN2"]); // these should be ignored when deleteAll toggled

        // Act
        await useCase.HandleRequestAsync(request);

        // Assert
        writeRepoDouble.Verify(repo =>
            repo.SaveMyPupilsAsync(
                user.UserId,
                It.Is<IEnumerable<UniquePupilNumber>>(list => !list.Any())),
            Times.Once);

        mockReadRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleRequestAsync_WhenSomeUpnsAreValid_SavesUpnsThatAreNotDeleted()
    {
        // Arrange

        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);
        User user = UserTestDoubles.WithUpns(upns);

        Mock<IUserReadOnlyRepository> mockReadRepository = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user);
        mockReadRepository
            .Setup((repo) =>
                repo.GetUserByIdAsync(
                    user.UserId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Mock<IUserWriteOnlyRepository> mockWriteRepository = UserWriteRepositoryTestDoubles.Default();
        DeletePupilsFromMyPupilsUseCase useCase = new(mockReadRepository.Object, mockWriteRepository.Object);

        IEnumerable<string> deletePupilUpnIdentifiers = user.UniquePupilNumbers.Take(1).Select(t => t.Value);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: user.UserId.Value,
            DeleteAll: false,
            DeletePupilUpns: deletePupilUpnIdentifiers);

        // Act
        await useCase.HandleRequestAsync(request);

        // Assert
        mockReadRepository.Verify(
            (repo) => repo.GetUserByIdAsync(
                user.UserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IEnumerable<UniquePupilNumber> expectedListAfterDelete = user.UniquePupilNumbers.Where(t => !deletePupilUpnIdentifiers.Contains(t.Value));

        mockWriteRepository.Verify(repo =>
            repo.SaveMyPupilsAsync(
                user.UserId,
                It.Is<IEnumerable<UniquePupilNumber>>(list => list.SequenceEqual(expectedListAfterDelete))),
            Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_WhenNoUpnsMatch_ThrowsArgumentException()
    {
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);
        User user = UserTestDoubles.WithUpns(upns);

        Mock<IUserReadOnlyRepository> mockReadRepository = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user);
        mockReadRepository
            .Setup((repo) =>
                repo.GetUserByIdAsync(
                    user.UserId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Mock<IUserWriteOnlyRepository> mockWriteRepository = UserWriteRepositoryTestDoubles.Default();
        DeletePupilsFromMyPupilsUseCase useCase = new(mockReadRepository.Object, mockWriteRepository.Object);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: user.UserId.Value,
            DeleteAll: false,
            DeletePupilUpns: ["UNKNOWN_UPN"]);

        // Act 
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleRequestAsync(request));

        // Assert
        mockReadRepository.Verify(
            (repo) => repo.GetUserByIdAsync(
                user.UserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockWriteRepository.Verify(
            (repo) => repo.SaveMyPupilsAsync(
                user.UserId,
                It.IsAny<IEnumerable<UniquePupilNumber>>()),
            Times.Never);
    }
}
