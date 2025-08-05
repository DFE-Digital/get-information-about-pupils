using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure.Repository;
public sealed class CosmosDbUserWriteRepositoryTests
{
    private const string UsersContainerName = "users";

    [Fact]
    public async Task SaveMyPupilsAsync_MapsUpnsAndCallsUpsert_WithExpectedDto()
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> commandHandlerDouble = new();
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
                It.Is<PartitionKey>((pk) => pk == new PartitionKey(userId.Value)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(EmptyInputs))]
    public async Task SaveMyPupilsAsync_WithNullUpns_UpsertsEmptyPupilList(IEnumerable<UniquePupilNumber>? upns)
    {
        // Arrange
        Mock<ICosmosDbCommandHandler> commandHandlerDouble = new();
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
                It.Is<PartitionKey>((pk) => pk == new PartitionKey(userId.Value)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }
    public static TheoryData<IEnumerable<UniquePupilNumber>?> EmptyInputs() => new(null, []);
}
