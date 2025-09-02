using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;

internal sealed class CosmosDbMyPupilsReadOnlyRepository : IMyPupilsReadOnlyRepository
{
    private const string ContainerName = "mypupils";
    private readonly ILogger<CosmosDbMyPupilsReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<MyPupilsDocumentDto, Application.Repositories.MyPupils> _myPupilsDtoToMyPupils;

    public CosmosDbMyPupilsReadOnlyRepository(
        ILogger<CosmosDbMyPupilsReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<MyPupilsDocumentDto, Application.Repositories.MyPupils> mapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;

        ArgumentNullException.ThrowIfNull(mapper);
        _myPupilsDtoToMyPupils = mapper;
    }

    public async Task<Application.Repositories.MyPupils?> GetMyPupilsOrDefaultAsync(UserId userId, CancellationToken ctx = default)
    {
        try
        {
            MyPupilsDocumentDto userDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<MyPupilsDocumentDto>(
                    id: userId.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: userId.Value,
                    ctx);

            if(userDto is null)
            {
                return null;
            }

            return _myPupilsDtoToMyPupils.Map(userDto);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation(ex, "Could not find MyPupils for User id {UserId}", userId.Value);
            return null;
        }

        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetMyPupilsOrDefaultAsync)}.");
            throw;
        }
    }
}
