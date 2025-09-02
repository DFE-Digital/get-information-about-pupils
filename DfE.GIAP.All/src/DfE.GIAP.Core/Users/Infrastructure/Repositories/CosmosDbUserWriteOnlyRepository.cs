using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories;
internal sealed class CosmosDbUserWriteOnlyRepository : IUserWriteOnlyRepository
{
    private const string ContainerName = "users";
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
                MyPupils = updatedMyPupils,
                // TODO: map other fields if needed
            };

            await _commandHandler.UpsertItemAsync(
                item: updatedUserProfile,
                containerKey: ContainerName,
                partitionKeyValue: userId.Value);
        }
        catch (CosmosException)
        {
            _logger.LogCritical("{method} Error in saving MyPupilsAsync for user: {userId}", nameof(SaveMyPupilsAsync), userId.Value);
            throw;
        }
    }

    public async Task UpsertUserAsync(User user)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(user);

            UserDto userDto = new()
            {
                id = user.UserId.Value,
                LastLoggedIn = user.LastLoggedIn,
                MyPupils = new MyPupilsDto
                {
                    Pupils = user.UniquePupilNumbers?.Select(upn => new MyPupilsItemDto
                    {
                        UPN = upn.Value
                    }) ?? []
                }
            };

            await _commandHandler.UpsertItemAsync(
                item: userDto,
                containerKey: ContainerName,
                partitionKeyValue: user.UserId.Value);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, "{method} Error upserting user: {userId}", nameof(UpsertUserAsync), user.UserId.Value);
            throw;
        }
    }
}
