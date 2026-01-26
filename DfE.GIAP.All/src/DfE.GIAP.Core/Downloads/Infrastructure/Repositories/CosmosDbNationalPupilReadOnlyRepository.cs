using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories;

public class CosmosDbNationalPupilReadOnlyRepository : INationalPupilReadOnlyRepository
{
    private const string ContainerKey = "pupil-noskill"; // This must match the key in the container options.
    private readonly IApplicationLoggerService _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<NationalPupilDto, NationalPupil> _dtoToEntityMapper;

    public CosmosDbNationalPupilReadOnlyRepository(
        IApplicationLoggerService logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<NationalPupilDto, NationalPupil> dtoToEntityMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(dtoToEntityMapper);
        _logger = logger;
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _dtoToEntityMapper = dtoToEntityMapper;
    }

    public async Task<IEnumerable<NationalPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds)
    {
        try
        {
            if (!pupilIds.Any())
                return [];

            IEnumerable<string> formattedIds = pupilIds.Select(id => $"'{id}'");
            string query = $"SELECT c.Census_Autumn,c.Census_Spring,c.Census_Summer,c.EYFSP,c.KS1,c.KS2,c.KS4,c.Phonics,c.MTC,c.PupilMatchingRef,c.DOB,c.UPN " +
                $"FROM c " +
                $"WHERE c.UPN IN ({string.Join(",", formattedIds)}) " +
                $"ORDER BY c.UPN ASC";
            IEnumerable<NationalPupilDto> queryResult = await _cosmosDbQueryHandler.ReadItemsAsync<NationalPupilDto>(ContainerKey, query);

            IEnumerable<NationalPupil> mappedResponse = queryResult.Select(_dtoToEntityMapper.Map);
            return mappedResponse;
        }
        catch (CosmosException ex)
        {
            _logger.LogTrace(
               level: LogLevel.Critical,
               message: $"CosmosException in {nameof(GetPupilsByIdsAsync)}",
               exception: ex,
               category: "Downloads - NPD",
               source: nameof(GetPupilsByIdsAsync));
            return [];
        }
    }
}
