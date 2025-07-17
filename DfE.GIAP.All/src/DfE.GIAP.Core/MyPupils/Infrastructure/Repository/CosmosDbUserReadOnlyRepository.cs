using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository.User;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repository;

internal sealed class CosmosDbUserReadOnlyRepository : IUserReadOnlyRepository
{
    private readonly ILogger<CosmosDbUserReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<UserProfileDto, User> _userMapper;

    public CosmosDbUserReadOnlyRepository(
        ILogger<CosmosDbUserReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<UserProfileDto, User> userMapper)
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
        IAuthorisationContext authorisationContext,
        CancellationToken ctx = default)
    {
        try
        {
            // Old query for UserProfile impl
            string query = $"SELECT * FROM u " +
                    $"WHERE u.DOCTYPE = 10 AND u.UserId = '{id.Value}' " +
                    $"ORDER BY u._ts DESC " +
                    $"OFFSET 0 LIMIT 1";


            IEnumerable<UserProfileDto> results = await _cosmosDbQueryHandler.ReadItemsAsync<UserProfileDto>(
                containerKey: "application-data",
                query,
                ctx);

            if (!results.Any())
            {
                throw new ArgumentException($"Unable to find User by identifier {id.Value}");
            }

            UserProfileDto userProfile = results.First();
            User user = _userMapper.Map(userProfile);
            return user;
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetUserByIdAsync)}.");
            throw;
        }
    }

}

