using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class CosmosDbUserWriteRepository : IUserWriteRepository
{
    private readonly ICosmosDbCommandHandler _commandHandler;

    public CosmosDbUserWriteRepository(ICosmosDbCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task SaveMyPupilsAsync(UserId userId, IEnumerable<UniquePupilNumber> updatedPupilIds)
    {
        IEnumerable<MyPupilsItemDto> updatedPupils = updatedPupilIds?.Select((upn) => new MyPupilsItemDto()
        {
            UPN = upn.Value
        }) ?? [];

        UserDto updatedUserProfile = new()
        {
            id = userId.Value,
            MyPupils = new MyPupilsDto
            {
                Pupils = updatedPupils
            }
            // TODO: map other fields if needed
        };

        await _commandHandler.UpsertItemAsync(
            item: updatedUserProfile,
            containerKey: "users",
            partitionKey: new PartitionKey(userId.Value));
    }
}
