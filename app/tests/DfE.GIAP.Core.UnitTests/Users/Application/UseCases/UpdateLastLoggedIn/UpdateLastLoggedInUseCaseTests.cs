using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLoggedIn;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Users.Application.UseCases.UpdateLastLoggedIn;

public sealed class UpdateLastLoggedInUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_UserReadOnlyRepositoryIsNull()
    {
        // Arrange
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UpdateLastLoggedInUseCase(
                userReadOnlyRepository: null!,
                userWriteOnlyRepository: writeRepo.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_UserWriteOnlyRepositoryIsNull()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UpdateLastLoggedInUseCase(
                userReadOnlyRepository: readRepo.Object,
                userWriteOnlyRepository: null!));
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        UpdateLastLoggedInUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.HandleRequestAsync(null!));
    }

    [Fact]
    public async Task HandleRequestAsync_CallsRepository_WithUpdatedLastLoggedIn()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        UpdateLastLoggedInUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        DateTime newLastLoggedIn = DateTime.UtcNow;
        UpdateLastLoggedInRequest request = new(userId.Value, newLastLoggedIn);
        User existingUser = UserTestDoubles.Default();

        readRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(existingUser);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        writeRepo.Verify(r => r.UpsertUserAsync(
            It.Is<User>(u => u.LastLoggedIn == newLastLoggedIn)), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_UserReadOnlyRepositoryThrows()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        UpdateLastLoggedInUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        UpdateLastLoggedInRequest request = new(userId.Value, DateTime.UtcNow);

        readRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ThrowsAsync(new Exception("Read repo exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            sut.HandleRequestAsync(request));
        writeRepo.Verify(r => r.UpsertUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_UserWriteOnlyRepositoryThrows()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        UpdateLastLoggedInUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        DateTime newLastLoggedIn = DateTime.UtcNow;
        UpdateLastLoggedInRequest request = new(userId.Value, newLastLoggedIn);
        User existingUser = UserTestDoubles.Default() with { LastLoggedIn = DateTime.MinValue };

        readRepo.Setup(r => r.GetUserByIdAsync(userId, default))
            .ReturnsAsync(existingUser);

        writeRepo.Setup(r => r.UpsertUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Write repo exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            sut.HandleRequestAsync(request));
    }
}
