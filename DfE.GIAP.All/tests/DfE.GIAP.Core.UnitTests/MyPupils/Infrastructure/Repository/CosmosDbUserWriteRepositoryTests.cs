using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure.Repository;
public sealed class CosmosDbUserWriteRepositoryTests
{
    private const string UsersContainerName = "users";

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_CommandHandlerIsNull()
    {
        // Arrange
        Func<CosmosDbUserWriteRepository> construct = () => new CosmosDbUserWriteRepository(
            commandHandler: null!,
            logger: LoggerTestDoubles.MockLogger<CosmosDbUserWriteRepository>());

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }


    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        Func<CosmosDbUserWriteRepository> construct = () => new CosmosDbUserWriteRepository(
            commandHandler: CosmosDbCommandHandlerTestDoubles.Default().Object,
            logger: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task SaveMyPupilsAsync_Throws_When_NonCosmosExceptionOccurs()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCosmosDbQueryHandler =
            CosmosDbCommandHandlerTestDoubles.MockThrowUpsertItemAsync<UserDto>(new Exception("test exception"));

        CosmosDbUserWriteRepository repository = new(
            commandHandler: mockCosmosDbQueryHandler.Object,
            logger: LoggerTestDoubles.MockLogger<CosmosDbUserWriteRepository>());

        // Act Assert
        await Assert.ThrowsAsync<Exception>(() =>
            repository.SaveMyPupilsAsync(
                UserIdTestDoubles.Default(),
                []));
    }

    [Fact]
    public async Task SaveMyPupilsAsync_LogsAndRethrows_When_CosmosExceptionIsThrown()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> mockCosmosDbQueryHandler =
            CosmosDbCommandHandlerTestDoubles.MockThrowUpsertItemAsync<UserDto>(
                CosmosExceptionTestDoubles.Default());

        InMemoryLogger<CosmosDbUserWriteRepository> inMemoryLogger = LoggerTestDoubles.MockLogger<CosmosDbUserWriteRepository>();

        CosmosDbUserWriteRepository repository = new(
            commandHandler: mockCosmosDbQueryHandler.Object,
            logger: inMemoryLogger);

        // Act Assert
        UserId userId = UserIdTestDoubles.Default();
        await Assert.ThrowsAsync<CosmosException>(() =>
            repository.SaveMyPupilsAsync(userId, []));

        string log = Assert.Single(inMemoryLogger.Logs);
        Assert.Contains($"SaveMyPupilsAsync Error in saving MyPupilsAsync for user: {userId.Value}", log);
    }

    [Theory]
    [MemberData(nameof(EmptyInputs))]
    public async Task SaveMyPupilsAsync_WithEmptyOrNullUpns_UpsertsEmptyPupilList(IEnumerable<UniquePupilNumber>? upns)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> commandHandlerDouble = CosmosDbCommandHandlerTestDoubles.Default();
        InMemoryLogger<CosmosDbUserWriteRepository> inMemoryLogger = LoggerTestDoubles.MockLogger<CosmosDbUserWriteRepository>();
        CosmosDbUserWriteRepository repository = new(commandHandlerDouble.Object, inMemoryLogger);

        UserId userId = UserIdTestDoubles.Default();

        // Act
        await repository.SaveMyPupilsAsync(userId, upns!);

        // Assert

        commandHandlerDouble.Verify(handler =>
            handler.UpsertItemAsync(
                It.Is<UserDto>(
                    (dto) => dto.id == userId.Value && dto.MyPupils.Pupils != null && !dto.MyPupils.Pupils.Any()),
                UsersContainerName,
                It.Is<string>((pk) => pk == userId.Value),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveMyPupilsAsync_MapsUpnsAndCallsUpsert_WithExpectedDto()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> commandHandlerDouble = CosmosDbCommandHandlerTestDoubles.Default();
        InMemoryLogger<CosmosDbUserWriteRepository> inMemoryLogger = LoggerTestDoubles.MockLogger<CosmosDbUserWriteRepository>();
        CosmosDbUserWriteRepository repository = new(commandHandlerDouble.Object, inMemoryLogger);

        UserId userId = UserIdTestDoubles.Default();
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);

        // Act
        await repository.SaveMyPupilsAsync(userId, upns);

        // Assert

        IEnumerable<string> expectedUpnsToWrite = upns.Select(t => t.Value);

        commandHandlerDouble.Verify(handler =>
            handler.UpsertItemAsync(
                It.Is<UserDto>(dto => dto.id == userId.Value && dto.MyPupils.Pupils.Select(p => p.UPN).SequenceEqual(expectedUpnsToWrite)),
                UsersContainerName,
                It.Is<string>((pk) => pk == userId.Value),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public static TheoryData<IEnumerable<UniquePupilNumber>?> EmptyInputs() => new(null, []);
}
