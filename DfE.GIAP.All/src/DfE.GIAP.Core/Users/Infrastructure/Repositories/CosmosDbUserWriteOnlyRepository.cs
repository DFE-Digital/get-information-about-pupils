using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories;

/// <summary>
/// Provides a write-only repository for managing user data in a Cosmos DB container.
/// </summary>
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

    /// <summary>
    /// Inserts or updates a user record in the database asynchronously.
    /// </summary>
    /// <remarks>This method ensures that the user record is either created or updated in the database,
    /// depending on whether the user already exists. The operation is performed in the context of the specified
    /// container and partition key.</remarks>
    /// <param name="user">The user object to be inserted or updated. The <see cref="User.UserId"/> property must have a value.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
            _logger.LogCritical(ex, $"CosmosException in {nameof(UpsertUserAsync)}.");
            throw;
        }
    }
}
