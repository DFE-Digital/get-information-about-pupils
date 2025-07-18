using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repository;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;
using User = DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository.User;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class CosmosDbUserReadOnlyRepositoryTests
{

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Mock<IMapper<UserProfileDto, User>> mockMapper = MapperTestDoubles.Default<UserProfileDto, User>();

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
        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserProfileDto, User>> mockMapper = MapperTestDoubles.Default<UserProfileDto, User>();

        Func<CosmosDbUserReadOnlyRepository> construct = () => new CosmosDbUserReadOnlyRepository(
            logger: mockLogger,
            cosmosDbQueryHandler: null!,
            userMapper: mockMapper.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_MapperIsNull()
    {
        // Arrange
        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.Default();

        Func<CosmosDbUserReadOnlyRepository> construct = () => new CosmosDbUserReadOnlyRepository(
            logger: mockLogger,
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
            CosmosDbQueryHandlerTestDoubles.MockForReadMany<UserProfileDto>(
                () => throw new Exception("test exception"));

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserProfileDto, User>> mockMapper = MapperTestDoubles.Default<UserProfileDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            repository.GetUserByIdAsync(
                new UserId(Guid.NewGuid().ToString()),
                It.IsAny<IAuthorisationContext>()));
    }

    [Fact]
    public async Task GetUserByIdAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadMany<UserProfileDto>(
                () => throw CosmosExceptionTestDoubles.Default());

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserProfileDto, User>> mockMapper = MapperTestDoubles.Default<UserProfileDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() =>
            repository.GetUserByIdAsync(
                new UserId(Guid.NewGuid().ToString()),
                It.IsAny<IAuthorisationContext>()));

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains("CosmosException in GetUserByIdAsync", log);

    }

    [Fact]
    public async Task GetUserByIdAsync_Throws_When_UserNotFound()
    {
        // Arrange

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadMany<UserProfileDto>(() => []);

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserProfileDto, User>> mockMapper = MapperTestDoubles.Default<UserProfileDto, User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.GetUserByIdAsync(
                new UserId(Guid.NewGuid().ToString()),
                It.IsAny<IAuthorisationContext>()));
    }


    [Fact]
    public async Task GetUserByIdAsync_ReturnsMappedUser_When_UserExists()
    {
        // Arrange
        const string applicationDataContainerName = "application-data";
        UserId userId = new("user");

        UserProfileDto userProfileDto = UserProfileDtoTestDoubles.WithId(userId);

        User expectedUser = new(
            userId,
            UniquePupilNumberTestDoubles.Generate(count: 3));

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadMany<UserProfileDto>(() => [userProfileDto]);

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserProfileDto, User>> mockMapper = MapperTestDoubles.Default<UserProfileDto, User>();

        mockMapper
            .Setup(m => m.Map(userProfileDto))
            .Returns(expectedUser);

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act
        User result = await repository.GetUserByIdAsync(userId, Mock.Of<IAuthorisationContext>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser, result);

        mockCosmosDbQueryHandler.Verify((handler) =>
            handler.ReadItemsAsync<UserProfileDto>(
                applicationDataContainerName,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);

        mockMapper.Verify(m => m.Map(userProfileDto), Times.Once);
    }

}
