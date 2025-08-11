using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;
using DfE.GIAP.Core.Users.Infrastructure.Repository;
using DfE.GIAP.Core.Users.Infrastructure.Repository.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Users.Infrastructure.Repository;
internal sealed class CosmosDbUserWriteOnlyRepository : IUserWriteOnlyRepository
{
    private readonly ICosmosDbCommandHandler _commandHandler;
    private readonly ILogger<CosmosDbUserWriteOnlyRepository> _logger;

    public CosmosDbUserWriteOnlyRepository(
        ICosmosDbCommandHandler commandHandler,
        ILogger<CosmosDbUserWriteOnlyRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(commandHandler);
        ArgumentNullException.ThrowIfNull(logger);
        _commandHandler = commandHandler;
        _logger = logger;
    }

    public async Task SaveMyPupilsAsync(UserId userId, IEnumerable<UniquePupilNumber> updatedPupilIds)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(userId);

            IEnumerable<MyPupilsItemDto> updatedPupils = updatedPupilIds?.Select((upn) => new MyPupilsItemDto()
            {
                UPN = upn.Value
            }) ?? [];

            MyPupilsDto updatedMyPupils = new()
            {
                Pupils = updatedPupils
            };

            UserDto updatedUserProfile = new()
            {
                id = userId.Value,
                MyPupils = updatedMyPupils
                // TODO: map other fields if needed
            };

            await _commandHandler.UpsertItemAsync(
                item: updatedUserProfile,
                containerKey: "users",
                partitionKeyValue: userId.Value);
        }
        catch (CosmosException)
        {
            _logger.LogCritical("{method} Error in saving MyPupilsAsync for user: {userId}", nameof(SaveMyPupilsAsync), userId.Value);
            throw;
        }
    }
}
