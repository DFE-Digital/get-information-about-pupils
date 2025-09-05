using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Users.Application.UseCases.CreateUserIfNotExists;
public sealed class CreateUserIfNotExistsUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_UserReadOnlyRepositoryIsNull()
    {
        // Arrange
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CreateUserIfNotExistsUseCase(
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
            new CreateUserIfNotExistsUseCase(
                userReadOnlyRepository: readRepo.Object,
                userWriteOnlyRepository: null!));
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        CreateUserIfNotExistsUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.HandleRequestAsync(null!));
    }

    [Fact]
    public async Task HandleRequestAsync_DoesNotCallsRepository_When_UserAlreadyExists()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        CreateUserIfNotExistsUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CreateUserIfNotExistsRequest request = new(userId.Value);
        User existingUser = UserTestDoubles.Default();

        readRepo.Setup(r => r.GetUserByIdIfExistsAsync(userId, default))
            .ReturnsAsync(existingUser);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        writeRepo.Verify(r => r.UpsertUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequestAsync_CallsRepository_When_UserDoesNotExist()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        CreateUserIfNotExistsUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CreateUserIfNotExistsRequest request = new(userId.Value);
        User existingUser = UserTestDoubles.Default();

        readRepo.Setup(r => r.GetUserByIdIfExistsAsync(userId, default))
            .ReturnsAsync((User?)null);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        writeRepo.Verify(r => r.UpsertUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_UserReadOnlyRepositoryThrows()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        CreateUserIfNotExistsUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CreateUserIfNotExistsRequest request = new(userId.Value);

        readRepo.Setup(r => r.GetUserByIdIfExistsAsync(userId, default))
            .ThrowsAsync(new Exception("Read repo exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            sut.HandleRequestAsync(request));
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_When_UserReadWriteRepositoryThrows()
    {
        // Arrange
        Mock<IUserReadOnlyRepository> readRepo = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IUserWriteOnlyRepository> writeRepo = UserWriteOnlyRepositoryTestDoubles.Default();
        CreateUserIfNotExistsUseCase sut = new(
            userReadOnlyRepository: readRepo.Object,
            userWriteOnlyRepository: writeRepo.Object);

        UserId userId = UserIdTestDoubles.Default();
        CreateUserIfNotExistsRequest request = new(userId.Value);

        readRepo.Setup(r => r.GetUserByIdIfExistsAsync(userId, default))
            .ThrowsAsync(new Exception("Read repo exception"));

        writeRepo.Setup(r => r.UpsertUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Write repo exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            sut.HandleRequestAsync(request));
    }
}
