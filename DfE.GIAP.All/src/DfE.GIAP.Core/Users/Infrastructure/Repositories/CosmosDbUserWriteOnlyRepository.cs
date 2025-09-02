using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
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

    public async Task UpsertUserAsync(User user)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(user);

            UserDto userDto = new()
            {
                id = user.UserId.Value,
                LastLoggedIn = user.LastLoggedIn,
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
