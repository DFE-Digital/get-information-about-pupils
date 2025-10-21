using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Infrastructure.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.UnitTests.Users.Infrastructure.Repositories;

public sealed class CosmosDbUserWriteOnlyRepositoryTests
{
    private readonly InMemoryLoggerService _mockLogger;

    public CosmosDbUserWriteOnlyRepositoryTests()
    {
        _mockLogger = LoggerServiceTestDoubles.MockLoggerService();
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullCommandHandler()
    {
        Mock<IMapper<User, UserDto>> mockMapper = MapperTestDoubles.Default<User, UserDto>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CosmosDbUserWriteOnlyRepository(
                commandHandler: null!,
                logger: _mockLogger,
                mapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullLogger()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = new();
        Mock<IMapper<User, UserDto>> mockMapper = MapperTestDoubles.Default<User, UserDto>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CosmosDbUserWriteOnlyRepository(
                commandHandler: mockCommandHandler.Object,
                logger: null!,
                mapper: mockMapper.Object));
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_ReceivesNullMapper()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CosmosDbUserWriteOnlyRepository(
                commandHandler: mockCommandHandler.Object,
                logger: _mockLogger,
                mapper: null!));
    }

    [Fact]
    public async Task UpsertUserAsync_ThrowsArgumentNullException_When_UserIsNull()
    {

        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<User, UserDto>> mockMapper = MapperTestDoubles.Default<User, UserDto>();
        CosmosDbUserWriteOnlyRepository sut = new(
            commandHandler: mockCommandHandler.Object,
            logger: _mockLogger,
            mapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpsertUserAsync(null!));
    }

    [Fact]
    public async Task UpsertUserAsync_Calls_CommandHandler_When_UserIsValid()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler = CosmosDbCommandHandlerTestDoubles.Default();
        Mock<IMapper<User, UserDto>> mockMapper = MapperTestDoubles.Default<User, UserDto>();
        CosmosDbUserWriteOnlyRepository sut = new(
            commandHandler: mockCommandHandler.Object,
            logger: _mockLogger,
            mapper: mockMapper.Object);

        // Act
        await sut.UpsertUserAsync(UserTestDoubles.Default());

        // Assert
        mockCommandHandler.Verify(m => m.UpsertItemAsync(It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task UpsertUserAsync_BubblesException_When_CosmosException()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCommandHandler =
            CosmosDbCommandHandlerTestDoubles.MockForUpsertItemAsyncThrows<UserDto>(
                CosmosExceptionTestDoubles.Default());
        Mock<IMapper<User, UserDto>> mockMapper = MapperTestDoubles.Default<User, UserDto>();

        CosmosDbUserWriteOnlyRepository sut = new(
            commandHandler: mockCommandHandler.Object,
            logger: _mockLogger,
            mapper: mockMapper.Object);

        // Act
        Func<Task> act = () => sut.UpsertUserAsync(UserTestDoubles.Default());

        // Act & Assert
        await Assert.ThrowsAsync<CosmosException>(act);
        Assert.Equal("CosmosException in UpsertUserAsync", _mockLogger.Logs.Single());
        mockCommandHandler.Verify(m => m.UpsertItemAsync(
            It.IsAny<UserDto>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }
}
