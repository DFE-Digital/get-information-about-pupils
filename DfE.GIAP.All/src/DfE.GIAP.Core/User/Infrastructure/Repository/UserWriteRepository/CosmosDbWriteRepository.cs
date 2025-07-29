using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserWriteRepository;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.User.Infrastructure.Repository.UserWriteRepository;
public sealed class CosmosDbUserWriteRepository : IUserWriteRepository
{
    private readonly ICosmosDbCommandHandler _commandHandler;

    public CosmosDbUserWriteRepository(ICosmosDbCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task SaveMyPupilsAsync(UserId userId, IEnumerable<UniquePupilNumber> updatedPupilIds)
    {
        IEnumerable<MyPupilItemDto> updatedPupils = updatedPupilIds?.Select(upn => new MyPupilItemDto
        {
            UPN = upn.Value
        }) ?? [];

        UserDto updatedUserProfile = new UserDto
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
