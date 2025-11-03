using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using Moq;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;

public sealed class DeletePupilsFromMyPupilsUseCaseTests
{
    [Fact]
    public async Task HandleRequestAsync_WhenDeleteAllIsTrue_SavesEmptyList()
    {
        // Arrange
        User user = UserTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, writeRepoMock.Object);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: user.UserId.Value,
            DeleteAll: true,
            DeletePupilUpns: UniquePupilNumberTestDoubles.Generate(count: 2)); // these should be ignored when deleteAll toggled

        // Act
        await useCase.HandleRequestAsync(request);

        // Assert
        writeRepoMock.Verify(repo =>
            repo.SaveMyPupilsAsync(
                user.UserId, It.Is<UniquePupilNumbers>(upns => !upns.GetUniquePupilNumbers().Any())),
                    Times.Once);

        readRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleRequestAsync_WhenSomeUpnsAreValid_SavesUpnsThatAreNotDeleted()
    {
        // Arrange

        UniquePupilNumbers upns =
            UniquePupilNumbers.Create(
                uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 3));

        UserId userId = UserIdTestDoubles.Default();

        Core.MyPupils.Application.Repositories.MyPupils myPupils = MyPupilsTestDoubles.Create(upns);

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockFor(myPupils);
        Mock<IMyPupilsWriteOnlyRepository> mockWriteRepository = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        IEnumerable<UniquePupilNumber> deletePupilUpnIdentifiers = upns.GetUniquePupilNumbers().Take(1);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: userId.Value,
            DeleteAll: false,
            DeletePupilUpns: deletePupilUpnIdentifiers);

        // Act
        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, mockWriteRepository.Object);
        await useCase.HandleRequestAsync(request);

        // Assert
        readRepositoryMock.Verify(
            (repo) => repo.GetMyPupilsOrDefaultAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        IEnumerable<UniquePupilNumber> expectedListAfterDelete = upns.GetUniquePupilNumbers().Where(t => !deletePupilUpnIdentifiers.Contains(t));

        mockWriteRepository.Verify(repo =>
            repo.SaveMyPupilsAsync(
                userId,
                It.Is<UniquePupilNumbers>(list => list.GetUniquePupilNumbers().SequenceEqual(expectedListAfterDelete))),
            Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_WhenNoUpnsMatch_ThrowsArgumentException()
    {
        // Arrange
        UniquePupilNumbers upns =
            UniquePupilNumbers.Create(uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 3));

        UserId userId = UserIdTestDoubles.Default();

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsWriteOnlyRepository> mockWriteRepository = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Core.MyPupils.Application.Repositories.MyPupils myPupils = new(upns);

        readRepositoryMock
            .Setup((repo) => repo.GetMyPupilsOrDefaultAsync(
                It.IsAny<UserId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(myPupils);

        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, mockWriteRepository.Object);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: userId.Value,
            DeleteAll: false,
            DeletePupilUpns: UniquePupilNumberTestDoubles.Generate(count: 1));

        // Act 
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleRequestAsync(request));

        // Assert
        readRepositoryMock.Verify(
            (repo) => repo.GetMyPupilsOrDefaultAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockWriteRepository.Verify(repo =>
            repo.SaveMyPupilsAsync(
                userId,
                It.IsAny<UniquePupilNumbers>()),
            Times.Never);
    }
}
