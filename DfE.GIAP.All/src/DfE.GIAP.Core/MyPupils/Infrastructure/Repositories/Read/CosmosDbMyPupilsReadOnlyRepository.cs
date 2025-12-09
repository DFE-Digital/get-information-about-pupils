using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;

internal sealed class CosmosDbMyPupilsReadOnlyRepository : IMyPupilsReadOnlyRepository
{
    private const string ContainerName = "mypupils";
    private readonly ILogger<CosmosDbMyPupilsReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly MyPupilsOptions _myPupilsOptions;

    public CosmosDbMyPupilsReadOnlyRepository(
        ILogger<CosmosDbMyPupilsReadOnlyRepository> logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IOptions<MyPupilsOptions> myPupilsOptions)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;

        ArgumentNullException.ThrowIfNull(myPupilsOptions);
        ArgumentNullException.ThrowIfNull(myPupilsOptions.Value);
        _myPupilsOptions = myPupilsOptions.Value;
    }

    public async Task<MyPupilsAggregate?> GetMyPupilsOrDefaultAsync(MyPupilsId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            MyPupilsDocumentDto? myPupilsDocumentDto =
                await _cosmosDbQueryHandler.TryReadItemByIdAsync<MyPupilsDocumentDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value);

            if (myPupilsDocumentDto is null)
            {
                _logger.LogInformation("Could not find MyPupils for User id {UserId}", id.Value);
                return null;
            }

            IEnumerable<UniquePupilNumber> myPupils =
                myPupilsDocumentDto.MyPupils.Pupils.Select(t => t.UPN).ToUniquePupilNumbers();

            return new MyPupilsAggregate(
                    id,
                    UniquePupilNumbers.Create(myPupils),
                    _myPupilsOptions.PupilsLimit);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetMyPupilsOrDefaultAsync)}.");
            return null;
        }
    }

    public async Task<MyPupilsAggregate> GetMyPupils(MyPupilsId id)
    {
        MyPupilsAggregate? myPupils = await GetMyPupilsOrDefaultAsync(id);

        return myPupils ?? new MyPupilsAggregate(
                id,
                UniquePupilNumbers.Create([]),
                _myPupilsOptions.PupilsLimit);
    }
}
