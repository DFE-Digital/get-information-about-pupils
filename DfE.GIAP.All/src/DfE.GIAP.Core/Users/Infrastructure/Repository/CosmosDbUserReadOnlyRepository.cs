using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;
using DfE.GIAP.Core.Users.Infrastructure.Repository.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Users.Infrastructure.Repository;
internal sealed class CosmosDbUserReadOnlyRepository : IUserReadOnlyRepository
{
    private const string ContainerName = "users";
    private readonly ILogger<CosmosDbUserReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<UserDto, Application.User> _userMapper;

    public CosmosDbUserReadOnlyRepository(
        ILogger<CosmosDbUserReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<UserDto, Application.User> userMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(userMapper);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _logger = logger;
        _userMapper = userMapper;
    }

    public async Task<Application.User> GetUserByIdAsync(
        UserId id,
        CancellationToken ctx = default)
    {
        try
        {
            // TODO: An assumption the users have been migrated to User container
            UserDto userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<UserDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value,
                    ctx);

            ArgumentNullException.ThrowIfNull(userDto);
            Application.User user = _userMapper.Map(userDto);

            return user;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetUserByIdAsync)}.");
            throw;
        }
    }

}
