using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Contents.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of <see cref="IContentReadOnlyRepository"/> for retrieving content documents.
/// </summary>
public sealed class CosmosDbContentReadOnlyRepository : IContentReadOnlyRepository
{
    private const int ContentDocumentType = 20;
    private const string ContainerName = "application-data";
    private readonly ILogger<CosmosDbContentReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IMapper<ContentDto?, Content> _contentDtoToContentMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDbContentReadOnlyRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="contentDtoToContentMapper">Mapper for converting DTOs to domain models.</param>
    /// <param name="cosmosDbQueryHandler">Handler for executing Cosmos DB queries.</param>
    public CosmosDbContentReadOnlyRepository(
        ILogger<CosmosDbContentReadOnlyRepository> logger,
        IMapper<ContentDto?, Content> contentDtoToContentMapper,
        ICosmosDbQueryHandler cosmosDbQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(contentDtoToContentMapper);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        _logger = logger;
        _contentDtoToContentMapper = contentDtoToContentMapper;
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
    }

    /// <inheritdoc/>
    public async Task<Content> GetContentByIdAsync(ContentKey id, CancellationToken ctx = default)
    {
        try
        {
            string query = $"SELECT * FROM c WHERE c.DOCTYPE = {ContentDocumentType} AND c.id = '{id.Value}'";
            IEnumerable<ContentDto> response = await _cosmosDbQueryHandler.ReadItemsAsync<ContentDto>(ContainerName, query, ctx);
            ContentDto? output = response.FirstOrDefault();
            return _contentDtoToContentMapper.Map(output);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(GetContentByIdAsync)}.");
            throw;
        }
    }
}
