using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Application.Repository.UserAggregate;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.MyPupils.Infrastructure;
internal sealed class CosmosDbUserAggregateWriteRepository : IUserAggregateWriteRepository
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly ICosmosDbCommandHandler _commandHandler;

    public CosmosDbUserAggregateWriteRepository(
        IUserReadOnlyRepository userReadOnlyRepository,
        ICosmosDbCommandHandler commandHandler)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _commandHandler = commandHandler;
    }

    public async Task SaveAsync(UserAggregateRoot userAggregate)
    {
        IReadOnlyCollection<PupilId> updatedPupilIds = userAggregate.GetUpdatedPupilIds();
        // TODO if User does not exist, then need to write with updated

        // Fetch User to persist updated UPNs on User - UserAggregate does not expose these
        User.Application.Repository.UserReadRepository.User originalUser =
            await _userReadOnlyRepository.GetUserByIdAsync(userAggregate.AggregateId);

        List<MyPupilItemDto> updatedPupilList =
            originalUser.PupilIds
                .Where((originalPupilIdentifiers) => updatedPupilIds.Contains(originalPupilIdentifiers.PupilId))
                .Select((pupilRetainedInList)
                    => new MyPupilItemDto()
                    {
                        Id = Guid.Parse(pupilRetainedInList.PupilId.Id),
                        UPN = pupilRetainedInList.UniquePupilNumber.Value
                    })
                .ToList();

        UserDto updatedUserProfile = new()
        {
            id = userAggregate.Identifier.Value,
            MyPupils = updatedPupilList,
            // TODO: map other fields if needed
        };

        await _commandHandler.ReplaceItemAsync(
            item: updatedUserProfile,
            itemId: userAggregate.Identifier.Value,
            containerKey: "users",
            partitionKey: new PartitionKey(userAggregate.Identifier.Value));
    }

}
