using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Infrastructure.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.UnitTests.Users.Infrastructure.Repositories;
public sealed class CosmosDbUserReadOnlyRepositoryTests
{
    private readonly InMemoryLogger<CosmosDbUserReadOnlyRepository> _mockLogger;

    public CosmosDbUserReadOnlyRepositoryTests()
    {
        _mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        Func<CosmosDbUserReadOnlyRepository> construct = () => new CosmosDbUserReadOnlyRepository(
            logger: null!,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_QueryHandlerIsNull()
    {
        // Arrange
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        Func<CosmosDbUserReadOnlyRepository> construct = () => new CosmosDbUserReadOnlyRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: null!,
            userMapper: mockMapper.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_MapperIsNull()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        Func<CosmosDbUserReadOnlyRepository> construct = () => new CosmosDbUserReadOnlyRepository(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task GetUserByIdAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(
                () => throw new Exception("test exception"));

        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            repository.GetUserByIdAsync(
                UserIdTestDoubles.Default()));
    }

    [Fact]
    public async Task GetUserByIdAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(
                () => throw CosmosExceptionTestDoubles.Default());

        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() =>
            repository.GetUserByIdAsync(
                UserIdTestDoubles.Default()));

        string log = Assert.Single(_mockLogger.Logs);
        Assert.Contains("CosmosException in GetUserByIdAsync", log);
    }

    [Fact]
    public async Task GetUserByIdAsync_Throws_When_UserNotFound()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(null!);
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            repository.GetUserByIdAsync(
                UserIdTestDoubles.Default()));
    }


    [Fact]
    public async Task GetUserByIdAsync_ReturnsMappedUser_When_UserExists()
    {
        // Arrange
        const string usersContainerName = "users";
        UserId userId = UserIdTestDoubles.Default();

        UserDto userProfileDto = UserDtoTestDoubles.Create(userId);

        User expectedUser = new(userId, DateTime.UtcNow);

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById(() => userProfileDto);

        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        mockMapper
            .Setup(m => m.Map(userProfileDto))
            .Returns(expectedUser);

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act
        User result = await repository.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser, result);

        mockCosmosDbQueryHandler.Verify((handler) =>
            handler.ReadItemByIdAsync<UserDto>(
                userId.Value,
                usersContainerName,
                userId.Value,
                It.IsAny<CancellationToken>()), Times.Once);

        mockMapper.Verify(m => m.Map(userProfileDto), Times.Once);
    }


    [Fact]
    public async Task GetUserByIdIfExistsAsync_ReturnsMappedUser_When_UserExists()
    {
        // Arrange
        const string usersContainerName = "users";
        UserId userId = UserIdTestDoubles.Default();

        UserDto userProfileDto = UserDtoTestDoubles.Create(userId);
        User expectedUser = new(userId, DateTime.UtcNow);

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById(() => userProfileDto);
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        mockMapper
            .Setup(m => m.Map(userProfileDto))
            .Returns(expectedUser);

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act
        User? result = await repository.GetUserByIdIfExistsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser, result);

        mockCosmosDbQueryHandler.Verify((handler) =>
            handler.ReadItemByIdAsync<UserDto>(
                userId.Value,
                usersContainerName,
                userId.Value,
                It.IsAny<CancellationToken>()), Times.Once);

        mockMapper.Verify(m => m.Map(userProfileDto), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdIfExistsAsync_ReturnsNull_When_UserDoesNotExist()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(null!);
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act
        User? result = await repository.GetUserByIdIfExistsAsync(userId);

        // Assert
        Assert.Null(result);
        mockMapper.Verify(m => m.Map(It.IsAny<UserDto>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdIfExistsAsync_ReturnsNull_When_CosmosExceptionIsThrownWithNotFound()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        CosmosException notFoundException = CosmosExceptionTestDoubles.WithStatusCode(System.Net.HttpStatusCode.NotFound);

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(() => throw notFoundException);
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act
        User? result = await repository.GetUserByIdIfExistsAsync(userId);

        // Assert
        Assert.Null(result);
        Assert.Contains("User with ID", _mockLogger.Logs.Single());
        mockMapper.Verify(m => m.Map(It.IsAny<UserDto>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdIfExistsAsync_ThrowsAndLogs_When_ExceptionIsThrown()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        Exception genericException = new Exception("test exception");

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(() => throw genericException);
        Mock<IMapper<UserDto, User>> mockMapper = MapperTestDoubles.Default<UserDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: _mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() =>
            repository.GetUserByIdIfExistsAsync(userId));

        Assert.Equal(genericException, ex);
        Assert.Contains("Exception in GetUserByIdIfExistsAsync", _mockLogger.Logs.Single());
    }
}
