using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Application.Repository.UserAggregate;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.MyPupils.Infrastructure;
internal sealed class CosmosDbUserAggregateWriteRepository : IUserAggregateWriteRepository
{
    private readonly ICosmosDbCommandHandler _commandHandler;

    public CosmosDbUserAggregateWriteRepository(
        ICosmosDbCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public async Task SaveAsync(UserAggregateRoot userAggregate)
    {
        IReadOnlyCollection<UniquePupilNumber> updatedPupilIds = userAggregate.GetPupilIds();
        IEnumerable<MyPupilItemDto> updatedPupils = updatedPupilIds.Select(t => new MyPupilItemDto()
        {
            UPN = t.Value
        });

        UserDto updatedUserProfile = new()
        {
            id = userAggregate.Identifier.Value,
            MyPupils = new MyPupilsDto()
            {
                Pupils = updatedPupils
            },
            // TODO: map other fields if needed
        };

        // TODO this handles if the User does not have a document already, but should that be a separate consideration?
        await _commandHandler.UpsertItemAsync(
            item: updatedUserProfile,
            containerKey: "users",
            partitionKey: new PartitionKey(userAggregate.Identifier.Value));
    }

}
