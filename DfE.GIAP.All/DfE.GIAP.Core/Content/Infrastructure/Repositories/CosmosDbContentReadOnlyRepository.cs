using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Options;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Options.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Content.Infrastructure.Repositories;
public sealed class CosmosDbContentReadOnlyRepository : IContentReadOnlyRepository
{
    // TODO these will be driven through options when ICosmosDbQueryHandler
    private const int ContentDocumentType = 20;
    private const string ContainerName = "application-data";
    private const string DatabaseId = "giap-search";
    private readonly CosmosClient _cosmosClient;
    private readonly ContentRepositoryOptions _contentRepositoryOptions;
    private readonly IMapper<ContentDTO, Application.Model.Content> _contentDtoToContentMapper;

    public CosmosDbContentReadOnlyRepository(
        CosmosClient cosmosClient,
        IMapper<ContentDTO, Application.Model.Content> contentDtoToContentMapper,
        IOptions<Options.ContentRepositoryOptions> contentRepositoryOptions)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(contentDtoToContentMapper);
        ArgumentNullException.ThrowIfNull(contentRepositoryOptions);
        _cosmosClient = cosmosClient;
        _contentDtoToContentMapper = contentDtoToContentMapper;
        _contentRepositoryOptions = contentRepositoryOptions.Value ?? throw new ArgumentNullException(nameof(contentRepositoryOptions.Value));
    }

    public async Task<Application.Model.Content?> GetContentByKeyAsync(ContentKey key, CancellationToken ctx = default)
    {
        Container container = _cosmosClient.GetContainer(
            databaseId: DatabaseId,
            containerId: ContainerName);

        string contentIdentifier = _contentRepositoryOptions.GetContentIdentifierFromKey(key);

        if (string.IsNullOrEmpty(contentIdentifier))
        {
            // TODO not throwing just returning null, no configuration registered for key
            // TODO log 
            return null;
        }

        QueryDefinition queryDefinition = new QueryDefinition(
         "SELECT * FROM c WHERE c.DOCTYPE = @doctype AND c.id = @id")
             .WithParameter("@doctype", ContentDocumentType)
             .WithParameter("@id", contentIdentifier);


        using FeedIterator<ContentDTO> resultSet = container.GetItemQueryIterator<ContentDTO>(queryDefinition, null, null);
        FeedResponse<ContentDTO> response = await resultSet.ReadNextAsync(ctx);
        ContentDTO? output = response.FirstOrDefault();

        if (output == null)
        {
            return null;
        }

        return _contentDtoToContentMapper.Map(output);
        
    }
}
