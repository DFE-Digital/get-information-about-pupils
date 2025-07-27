using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.SharedTests.TestDoubles.Users;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.User.Infrastructure;
public sealed class CosmosDbUserReadOnlyRepositoryTests
{

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Mock<IMapper<UserDto, Core.User.Application.Repository.UserReadRepository.User>> mockMapper = MapperTestDoubles.Default<UserDto, Core.User.Application.Repository.UserReadRepository.User>();

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

        Mock<IMapper<UserDto, Core.User.Application.Repository.UserReadRepository.User>> mockMapper = MapperTestDoubles.Default<UserDto, Core.User.Application.Repository.UserReadRepository.User>();

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
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(
                () => throw new Exception("test exception"));

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserDto, Core.User.Application.Repository.UserReadRepository.User>> mockMapper = MapperTestDoubles.Default<UserDto, Core.User.Application.Repository.UserReadRepository.User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            repository.GetUserByIdAsync(
                new UserId(Guid.NewGuid().ToString())));
    }

    [Fact]
    public async Task GetUserByIdAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(
                () => throw CosmosExceptionTestDoubles.Default());

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserDto, Core.User.Application.Repository.UserReadRepository.User>> mockMapper = MapperTestDoubles.Default<UserDto, Core.User.Application.Repository.UserReadRepository.User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAsync<CosmosException>(() =>
            repository.GetUserByIdAsync(
                new UserId(Guid.NewGuid().ToString())));

        string log = Assert.Single(mockLogger.Logs);
        Assert.Contains("CosmosException in GetUserByIdAsync", log);

    }

    [Fact]
    public async Task GetUserByIdAsync_Throws_When_UserNotFound()
    {
        // Arrange
        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler = CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(null!);

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserDto, Core.User.Application.Repository.UserReadRepository.User>> mockMapper = MapperTestDoubles.Default<UserDto, Core.User.Application.Repository.UserReadRepository.User>();

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            repository.GetUserByIdAsync(
                new UserId(Guid.NewGuid().ToString())));
    }


    [Fact]
    public async Task GetUserByIdAsync_ReturnsMappedUser_When_UserExists()
    {
        // Arrange
        const string usersContainerName = "users";
        UserId userId = new("user");

        UserDto userProfileDto = UserDtoTestDoubles.WithId(userId);

        List<PupilIdentifier> pupilIdentifiers =
            UniquePupilNumberTestDoubles.Generate(count: 3)
                .Select((upn)
                    => new PupilIdentifier(
                        new PupilId(Guid.NewGuid()),
                        upn))
                .ToList();

        Core.User.Application.Repository.UserReadRepository.User expectedUser = new(
            userId,
            pupilIdentifiers);

        Mock<ICosmosDbQueryHandler> mockCosmosDbQueryHandler =
            CosmosDbQueryHandlerTestDoubles.MockForReadById<UserDto>(() => userProfileDto);

        InMemoryLogger<CosmosDbUserReadOnlyRepository> mockLogger = LoggerTestDoubles.MockLogger<CosmosDbUserReadOnlyRepository>();

        Mock<IMapper<UserDto, Core.User.Application.Repository.UserReadRepository.User>> mockMapper = MapperTestDoubles.Default<UserDto, Core.User.Application.Repository.UserReadRepository.User>();

        mockMapper
            .Setup(m => m.Map(userProfileDto))
            .Returns(expectedUser);

        CosmosDbUserReadOnlyRepository repository = new(
            logger: mockLogger,
            cosmosDbQueryHandler: mockCosmosDbQueryHandler.Object,
            userMapper: mockMapper.Object);

        // Act
        Core.User.Application.Repository.UserReadRepository.User result = await repository.GetUserByIdAsync(userId);

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

}
