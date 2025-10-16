using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using Microsoft.Azure.Cosmos;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories;

/// <summary>
/// Provides a write-only repository for managing user data in a Cosmos DB container.
/// </summary>
internal sealed class CosmosDbUserWriteOnlyRepository : IUserWriteOnlyRepository
{
    private const string ContainerName = "users";
    private readonly ICosmosDbCommandHandler _commandHandler;
    private readonly ILoggerService _loggerService;
    private readonly IMapper<User, UserDto> _mapper;

    public CosmosDbUserWriteOnlyRepository(
        ICosmosDbCommandHandler commandHandler,
        ILoggerService loggerService,
        IMapper<User, UserDto> mapper)
    {
        ArgumentNullException.ThrowIfNull(commandHandler);
        ArgumentNullException.ThrowIfNull(loggerService);
        ArgumentNullException.ThrowIfNull(mapper);
        _commandHandler = commandHandler;
        _loggerService = loggerService;
        _mapper = mapper;
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
            UserDto userDto = _mapper.Map(user);

            await _commandHandler.UpsertItemAsync(
                item: userDto,
                containerKey: ContainerName,
                partitionKeyValue: user.UserId.Value);
        }
        catch (CosmosException ex)
        {
            _loggerService.LogTrace(
                level: LogLevel.Critical,
                message: $"CosmosException in {nameof(UpsertUserAsync)}.",
                exception: ex,
                category: "Users",
                source: nameof(UpsertUserAsync));
            throw;
        }
    }
}
