using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Provider;
using DfE.GIAP.Core.Content.Application.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories;
public sealed class CosmosDbContentReadOnlyRepository : IContentReadOnlyRepository
{
    private const int ContentDocumentType = 20;
    private const string ContainerName = "application-data";
    private readonly ILogger<CosmosDbContentReadOnlyRepository> _logger;
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler; 
    private readonly IMapper<ContentDTO?, Application.Model.Content> _contentDtoToContentMapper;
    private readonly IPageContentOptionsProvider _pageContentOptionsProvider;

    public CosmosDbContentReadOnlyRepository(
        ILogger<CosmosDbContentReadOnlyRepository> logger,
        IMapper<ContentDTO?, Application.Model.Content> contentDtoToContentMapper,
        IPageContentOptionsProvider pageContentOptionProvider,
        ICosmosDbQueryHandler cosmosDbQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(contentDtoToContentMapper);
        ArgumentNullException.ThrowIfNull(pageContentOptionProvider);
        ArgumentNullException.ThrowIfNull(cosmosDbQueryHandler);
        _logger = logger;
        _contentDtoToContentMapper = contentDtoToContentMapper;
        _pageContentOptionsProvider = pageContentOptionProvider;
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
    }

    public async Task<Application.Model.Content> GetContentByKeyAsync(ContentKey key, CancellationToken ctx = default)
    {
        try
        {
            PageContentOption options = _pageContentOptionsProvider.GetPageContentOptionWithPageKey(key.Value);

            if(string.IsNullOrWhiteSpace(options.DocumentId))
            {
                throw new ArgumentException($"DocumentId is null or whitespace for key {key.Value}");
            }

            string query = $"SELECT * FROM c WHERE c.DOCTYPE = {ContentDocumentType} AND c.id = '{options.DocumentId}'";
            IEnumerable<ContentDTO> response = await _cosmosDbQueryHandler.ReadItemsAsync<ContentDTO>(ContainerName, query, ctx);
            ContentDTO? output = response.FirstOrDefault();
            return _contentDtoToContentMapper.Map(output);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(CosmosDbContentReadOnlyRepository.GetContentByKeyAsync)}.");
            throw;
        }
    }
}
