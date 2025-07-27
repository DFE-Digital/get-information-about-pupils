using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.User.Infrastructure.Repository;

internal sealed class CosmosDbUserReadOnlyRepository : IUserReadOnlyRepository
{
    private readonly ILogger<CosmosDbUserReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<UserDto, Application.Repository.UserReadRepository.User> _userMapper;

    public CosmosDbUserReadOnlyRepository(
        ILogger<CosmosDbUserReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<UserDto, Application.Repository.UserReadRepository.User> userMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(userMapper);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _logger = logger;
        _userMapper = userMapper;
    }

    public async Task<Application.Repository.UserReadRepository.User> GetUserByIdAsync(
        UserId id,
        CancellationToken ctx = default)
    {
        try
        {
            UserDto userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<UserDto>(
                    id: id.Value,
                    containerKey: "users",
                    partitionKeyValue: id.Value,
                    ctx);

            ArgumentNullException.ThrowIfNull(userDto);
            
            Application.Repository.UserReadRepository.User user = _userMapper.Map(userDto);
            return user;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetUserByIdAsync)}.");
            throw;
        }
    }

}

