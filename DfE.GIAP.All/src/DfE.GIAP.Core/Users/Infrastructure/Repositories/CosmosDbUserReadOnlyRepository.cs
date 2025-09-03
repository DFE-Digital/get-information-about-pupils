using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = DfE.GIAP.Core.Users.Application.User;

namespace DfE.GIAP.Core.Users.Infrastructure.Repositories;
internal sealed class CosmosDbUserReadOnlyRepository : IUserReadOnlyRepository
{
    private const string ContainerName = "users";
    private readonly ILogger<CosmosDbUserReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<UserDto, User> _userMapper;

    public CosmosDbUserReadOnlyRepository(
        ILogger<CosmosDbUserReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<UserDto, User> userMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(userMapper);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _logger = logger;
        _userMapper = userMapper;
    }

    public async Task<User> GetUserByIdAsync(
        UserId id,
        CancellationToken ctx = default)
    {
        try
        {
            UserDto userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<UserDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value,
                    ctx);

            ArgumentNullException.ThrowIfNull(userDto);
            User user = _userMapper.Map(userDto);

            return user;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetUserByIdAsync)}.");
            throw;
        }
    }

    public async Task<User?> GetUserByIdIfExistsAsync(
        UserId id,
        CancellationToken ctx = default)
    {
        try
        {
            UserDto? userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<UserDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value,
                    ctx);

            return userDto is not null ? _userMapper.Map(userDto) : null;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("User with ID '{UserId}' not found (404).", id.Value);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception in {nameof(GetUserByIdIfExistsAsync)}");
            throw;
        }
    }
}
