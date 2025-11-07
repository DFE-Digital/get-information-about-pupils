using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories;

public class CosmosDbFurtherEducationReadOnlyRepository : IFurtherEducationReadOnlyRepository
{
    private const string ContainerKey = "further-education"; // This must match the key in the container options.
    private readonly IApplicationLogger _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<FurtherEducationPupilDto, FurtherEducationPupil> _dtoToEntityMapper;

    public CosmosDbFurtherEducationReadOnlyRepository(
        IApplicationLogger logger,
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IMapper<FurtherEducationPupilDto, FurtherEducationPupil> dtoToEntityMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        ArgumentNullException.ThrowIfNull(dtoToEntityMapper);
        _logger = logger;
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _dtoToEntityMapper = dtoToEntityMapper;
    }

    public async Task<IEnumerable<FurtherEducationPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds)
    {
        try
        {
            if (!pupilIds.Any())
                return [];

            IEnumerable<string> formattedIds = pupilIds.Select(id => $"'{id}'");
            string query = $"SELECT * FROM c WHERE c.ULN IN ({string.Join(",", formattedIds)})";
            IEnumerable<FurtherEducationPupilDto> queryResult = await _cosmosDbQueryHandler.ReadItemsAsync<FurtherEducationPupilDto>(ContainerKey, query);

            IEnumerable<FurtherEducationPupil> mappedResponse = queryResult.Select(_dtoToEntityMapper.Map);
            return mappedResponse;
        }
        catch (CosmosException ex)
        {
            _logger.LogTrace(
                level: LogLevel.Critical,
                message: $"CosmosException in {nameof(GetPupilsByIdsAsync)}",
                exception: ex,
                category: "Downloads - FE",
                source: nameof(GetPupilsByIdsAsync));
            return [];
        }
    }
}
