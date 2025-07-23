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

        // Fetch User to retrieve UniquePupilNumbers without Upn masking
        User.Application.Repository.UserReadRepository.User originalUser =
            await _userReadOnlyRepository.GetUserByIdAsync(userAggregate.AggregateId);

        // TODO stub out the PupilDtos on UserReadOnlyRepository to have static UPNs and Static PupilIds.
        // It's being done on MapUserProfileDtoToUserMapper. So rather than being dynamic it pulls a static list, then test removal.

        List<PupilItemDto> updatedPupilList =
            originalUser.PupilIds
                .Where(
                    (originalPupilIdentifiers) => // Select matching identifiers in the list
                        updatedPupilIds.Any(id => id == originalPupilIdentifiers.PupilId))
                .Select((pupilRetainedInList)
                    => new PupilItemDto()
                    {
                        Id = Guid.Parse(pupilRetainedInList.PupilId.Id),
                        PupilId = pupilRetainedInList.UniquePupilNumber.Value
                    })
                .ToList();

        UserProfileDto updatedUserProfile = new()
        {
            UserId = userAggregate.Identifier.Value,
            MyPupilList = updatedPupilList,
            // TODO: map other fields if needed
        };

        await _commandHandler.ReplaceItemAsync(
            item: updatedUserProfile,
            itemId: userAggregate.Identifier.Value,
            containerKey: "application-data",
            partitionKey: new PartitionKey(10));
    }

}
