﻿using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
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

    public async Task<Domain.AggregateRoot.MyPupils?> GetMyPupilsOrDefaultAsync(MyPupilsId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            MyPupilsDocumentDto myPupilsDocumentDto =
                await _cosmosDbQueryHandler.ReadItemByIdAsync<MyPupilsDocumentDto>(
                    id: id.Value,
                    containerKey: ContainerName,
                    partitionKeyValue: id.Value);

            if (myPupilsDocumentDto is null)
            {
                return null;
            }

            IEnumerable<UniquePupilNumber> myPupils =
                myPupilsDocumentDto.MyPupils.Pupils.Select(t => t.UPN).ToUniquePupilNumbers();

            return new
                Domain.AggregateRoot.MyPupils(
                    id,
                    UniquePupilNumbers.Create(myPupils),
                    _myPupilsOptions.PupilsLimit);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation(ex, "Could not find MyPupils for User id {UserId}", id.Value);
            return null;
        }

        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetMyPupilsOrDefaultAsync)}.");
            throw;
        }
    }

    public async Task<Domain.AggregateRoot.MyPupils> GetMyPupils(MyPupilsId id)
    {
        Domain.AggregateRoot.MyPupils? myPupils = await GetMyPupilsOrDefaultAsync(id);

        return myPupils
            ?? new Domain.AggregateRoot.MyPupils(
                id,
                UniquePupilNumbers.Create([]),
                _myPupilsOptions.PupilsLimit);
    }
}
