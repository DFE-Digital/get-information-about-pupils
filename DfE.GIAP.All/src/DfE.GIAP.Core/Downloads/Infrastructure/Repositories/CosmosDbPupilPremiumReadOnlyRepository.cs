using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories;

public class CosmosDbPupilPremiumReadOnlyRepository : IPupilPremiumReadOnlyRepository
{
    private const string ContainerKey = "pupil-premium"; // This must match the key in the container options.
    private readonly IApplicationLoggerService _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<PupilPremiumPupilDto, PupilPremiumPupil> _dtoToEntityMapper;

    public CosmosDbPupilPremiumReadOnlyRepository(
        IApplicationLoggerService logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<PupilPremiumPupilDto, PupilPremiumPupil> dtoToEntityMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(dtoToEntityMapper);
        _logger = logger;
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _dtoToEntityMapper = dtoToEntityMapper;
    }

    public async Task<IEnumerable<PupilPremiumPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds)
    {
        try
        {
            if (!pupilIds.Any())
                return [];

            IEnumerable<string> formattedIds = pupilIds.Select(id => $"'{id}'");
            string query = $"SELECT * FROM c WHERE c.UPN IN ({string.Join(",", formattedIds)})";
            IEnumerable<PupilPremiumPupilDto> queryResult = await _cosmosDbQueryHandler.ReadItemsAsync<PupilPremiumPupilDto>(ContainerKey, query);

            IEnumerable<PupilPremiumPupil> mappedResponse = queryResult.Select(_dtoToEntityMapper.Map);
            return mappedResponse;
        }
        catch (CosmosException ex)
        {
            _logger.LogTrace(
               level: LogLevel.Critical,
               message: $"CosmosException in {nameof(GetPupilsByIdsAsync)}",
               exception: ex,
               category: "Downloads - PP",
               source: nameof(GetPupilsByIdsAsync));
            return [];
        }
    }
}
