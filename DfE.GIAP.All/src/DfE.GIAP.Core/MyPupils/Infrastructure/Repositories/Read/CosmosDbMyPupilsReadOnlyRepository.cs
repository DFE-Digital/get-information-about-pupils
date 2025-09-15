using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
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

    public async Task<Domain.AggregateRoot.MyPupils?> GetMyPupilsOrDefaultAsync(UserId userId, CancellationToken ctx = default)
    {
        try
        {
            MyPupilsDocumentDto myPupilsDocumentDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<MyPupilsDocumentDto>(
                    id: userId.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: userId.Value,
                    ctx);

            if (myPupilsDocumentDto is null)
            {
                return null;
            }

            IEnumerable<UniquePupilNumber> myPupils =
                myPupilsDocumentDto.MyPupils.Pupils.Select(t => t.UPN).ToUniquePupilNumbers();

            return new
                Domain.AggregateRoot.MyPupils(
                    userId,
                    UniquePupilNumbers.Create(myPupils),
                    _myPupilsOptions.PupilsLimit);
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

    public async Task<Domain.AggregateRoot.MyPupils> GetMyPupils(UserId userId, CancellationToken ctx = default)
    {
        Domain.AggregateRoot.MyPupils? myPupils = await GetMyPupilsOrDefaultAsync(userId, ctx);

        return myPupils
            ?? new Domain.AggregateRoot.MyPupils(
                userId,
                UniquePupilNumbers.Create([]),
                _myPupilsOptions.PupilsLimit);
    }
}
