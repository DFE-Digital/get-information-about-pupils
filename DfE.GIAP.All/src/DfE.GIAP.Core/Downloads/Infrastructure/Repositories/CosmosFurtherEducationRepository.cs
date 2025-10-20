using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories;

public class CosmosFurtherEducationRepository : IFurtherEducationRepository
{
    private const string ContainerKey = "further-education"; // This must match the key in the container options.
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    public CosmosFurtherEducationRepository(ICosmosDbQueryHandler cosmosDbQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
    }

    public async Task<IEnumerable<FurtherEducationPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds)
    {
        string query = $"SELECT * FROM c WHERE c.ULN IN ({string.Join(",", pupilIds.Select(id => $"'{id}'"))})";
        IEnumerable<FurtherEducationPupil> result = await _cosmosDbQueryHandler.ReadItemsAsync<FurtherEducationPupil>(ContainerKey, query);
        return result;
    }
}
