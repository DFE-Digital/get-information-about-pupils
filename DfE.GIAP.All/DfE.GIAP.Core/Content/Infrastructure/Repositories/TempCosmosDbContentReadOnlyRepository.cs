using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Options;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Options.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories;
public sealed class TempCosmosDbContentReadOnlyRepository : IContentReadOnlyRepository
{
    // TODO these will be driven through options when ICosmosDbQueryHandler
    private const int ContentDocumentType = 20;
    private const string ContainerName = "application-data";
    private const string DatabaseId = "giapsearch";
    private readonly ILogger<TempCosmosDbContentReadOnlyRepository> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly ContentRepositoryOptions _contentRepositoryOptions;
    private readonly IMapper<ContentDTO, Application.Model.Content> _contentDtoToContentMapper;

    public TempCosmosDbContentReadOnlyRepository(
        ILogger<TempCosmosDbContentReadOnlyRepository> logger,
        CosmosClient cosmosClient,
        IMapper<ContentDTO, Application.Model.Content> contentDtoToContentMapper,
        IOptions<ContentRepositoryOptions> contentRepositoryOptions)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(contentDtoToContentMapper);
        ArgumentNullException.ThrowIfNull(contentRepositoryOptions);
        _logger = logger;
        _cosmosClient = cosmosClient;
        _contentDtoToContentMapper = contentDtoToContentMapper;
        _contentRepositoryOptions = contentRepositoryOptions.Value ??
            throw new ArgumentNullException(nameof(contentRepositoryOptions.Value));
    }

    public async Task<Application.Model.Content> GetContentByKeyAsync(ContentKey key, CancellationToken ctx = default)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(
            databaseId: DatabaseId,
            containerId: ContainerName);

            string contentDocumentIdentifier = _contentRepositoryOptions.GetContentDocumentIdentifierFromKey(key);

            if (string.IsNullOrEmpty(contentDocumentIdentifier))
            {
                throw new ArgumentException($"Unable to find content document identifier for {key.Value}");
            }

            QueryDefinition queryDefinition = new QueryDefinition(
             "SELECT * FROM c WHERE c.DOCTYPE = @doctype AND c.id = @id")
                 .WithParameter("@doctype", ContentDocumentType)
                 .WithParameter("@id", contentDocumentIdentifier);


            using FeedIterator<ContentDTO> resultSet = container.GetItemQueryIterator<ContentDTO>(queryDefinition, null, null);
            FeedResponse<ContentDTO> response = await resultSet.ReadNextAsync(ctx);
            ContentDTO? output = response.FirstOrDefault();

            if (output == null)
            {
                return Application.Model.Content.Empty();
            }

            return _contentDtoToContentMapper.Map(output);
        }
        catch (CosmosException ex)
        {
            _logger.LogCritical(ex, $"CosmosException in {nameof(TempCosmosDbContentReadOnlyRepository.GetContentByKeyAsync)}.");
            return Application.Model.Content.Empty();
        }
    }
}
