using System.Net;
using System.Text;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Xunit;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;


// TODO composition alternative CosmosDbContext - contains the Client creation and operations, which is passed into each TestDatabase
public class DefaultCosmosDbTestDatabase : IAsyncDisposable
{
    private readonly string _databaseName;
    private readonly IReadOnlyList<CosmosDbContainerOptions> _containerOptions;
    private const string ApplicationDataContainerName = "application-data";
    private readonly CosmosClient _cosmosClient;

    public DefaultCosmosDbTestDatabase(
        Uri cosmosEndpoint,
        string? cosmosAuthKey,
        CosmosDbDatabaseOptions options)
    {
        if (!cosmosEndpoint.IsAbsoluteUri)
        {
            throw new ArgumentException("The Cosmos DB endpoint must be an absolute URI.", nameof(cosmosEndpoint));
        }

        _cosmosClient = new CosmosClient(
            accountEndpoint: cosmosEndpoint.ToString(),
            authKeyOrResourceToken: cosmosAuthKey ?? string.Empty,
            clientOptions: new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Direct
                // Consider in future if necessary - requires better control of docker emulator args, performance cost
                // ConsistencyLevel = ConsistencyLevel.Strong
            });

        _databaseName = options.DatabaseName;
        _containerOptions = options.Containers;
    }

    public async Task StartAsync()
    {
        DatabaseResponse db = await _cosmosClient!.CreateDatabaseIfNotExistsAsync(_databaseName);
        await CreateAllContainersIfNotExistAsync(db);
    }


    public async ValueTask DisposeAsync()
    {
        using (_cosmosClient)
        {
            await DeleteDatabase();
        }
    }

    public async Task ClearDatabaseAsync()
    {
        DatabaseResponse response = await _cosmosClient!.CreateDatabaseIfNotExistsAsync(_databaseName);
        List<ContainerResponse> containers = await CreateAllContainersIfNotExistAsync(response);

        foreach (ContainerResponse container in containers)
        {
            QueryDefinition queryDefinition = new("SELECT * FROM c");
            FeedIterator<dynamic> queryIterator = container.Container.GetItemQueryIterator<dynamic>(queryDefinition);
            List<Task> deleteTasks = [];

            while (queryIterator.HasMoreResults)
            {
                FeedResponse<dynamic> queriedItem = await queryIterator.ReadNextAsync();
                foreach (dynamic item in queriedItem)
                {
                    JObject itemObject = JObject.FromObject(item);
                    (string id, PartitionKey pk) = ExtractDocumentQueryValues(container, itemObject);
                    deleteTasks.Add(container.Container.DeleteItemAsync<dynamic>(id, pk));
                }
            }
            await Task.WhenAll(deleteTasks);
            await container.Container.ReadContainerAsync();
        }
    }

    public async Task DeleteDatabase() => await _cosmosClient!.GetDatabase(_databaseName).DeleteAsync();

    public async Task<IEnumerable<TDto>> ReadManyAsync<TDto>() where TDto : class
    {
        ContainerResponse targetContainer = await GetTargetContainerForDto<TDto>();

        List<TDto> output = [];
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        FeedIterator<TDto> iterator = targetContainer.Container.GetItemQueryIterator<TDto>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            FeedResponse<TDto> item = await iterator.ReadNextAsync();
            output.AddRange(item.Resource);
        }

        return output;
    }


    public async Task<IEnumerable<TDto>> ReadManyAsync<TDto>(IEnumerable<string> identifiers) where TDto : class
    {
        ContainerResponse targetContainer = await GetTargetContainerForDto<TDto>();
        Container? container = targetContainer.Container;
        List<string> targetIdentifiers = identifiers?.ToList() ?? [];

        // Build the query string
        QueryDefinition query = CreateQueryDefinitionForIdentifiers(targetIdentifiers);

        List<TDto> results = [];
        FeedIterator<TDto>? iterator = container.GetItemQueryIterator<TDto>(query);

        while (iterator.HasMoreResults)
        {
            FeedResponse<TDto>? response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        // NOTE does not guard if an identifier is not found
        return results;
    }


    public async Task WriteItemAsync<TDto>(TDto item) where TDto : class
    {
        ContainerResponse targetContainer = await GetTargetContainerForDto<TDto>();

        (string documentId, PartitionKey documentPartitionKey) = ExtractDocumentQueryValues(targetContainer, JObject.FromObject(item));

        await CreateItemInternalAsync(item, targetContainer);

        await EnsureItemIsQueryableAsync<TDto>(
                targetContainer.Container,
                documentId,
                documentPartitionKey);
    }


    public async Task WriteManyAsync<TDto>(IEnumerable<TDto> items) where TDto : class
    {
        ContainerResponse container = await GetTargetContainerForDto<TDto>();

        List<TDto> createItems = items?.ToList() ?? [];

        foreach (TDto item in createItems)
        {
            await CreateItemInternalAsync(item, container);

            (string id, PartitionKey key) = ExtractDocumentQueryValues(container, JObject.FromObject(item));
            await EnsureItemIsQueryableAsync<TDto>(
                container,
                id,
                key);
        }

    }

    private async Task<ContainerResponse> GetTargetContainerForDto<TDto>() where TDto : class
    {
        Dictionary<Type, string> typeToContainerNameMap = new()
        {
            {  typeof(NewsArticleDto), "news" },
            {  typeof(UserDto), "users" },
            {  typeof(MyPupilsDocumentDto), "mypupils" }
        };

        DatabaseResponse db = await _cosmosClient!.CreateDatabaseIfNotExistsAsync(_databaseName);
        List<ContainerResponse> containers = await CreateAllContainersIfNotExistAsync(db);

        typeToContainerNameMap.TryGetValue(typeof(TDto), out string? containerName);

        string targetContainer = containerName ?? ApplicationDataContainerName; // Default back to ApplicationData for undefined mappings
        return containers.Single((container) => container.Container.Id == targetContainer);
    }

    private async Task<List<ContainerResponse>> CreateAllContainersIfNotExistAsync(Database database)
    {
        ArgumentNullException.ThrowIfNull(database);
        List<ContainerResponse> containerResponses = [];

        foreach (CosmosDbContainerOptions options in _containerOptions)
        {
            ContainerResponse applicationData = await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
            {
                Id = options.ContainerName,
                PartitionKeyPath = options.PartitionKey,
            });
            containerResponses.Add(applicationData);
        }

        return containerResponses;
    }

    private static async Task CreateItemInternalAsync<T>(T obj, ContainerResponse container) where T : class
    {
        (string _, PartitionKey documentPartitionKey) = ExtractDocumentQueryValues(container, JObject.FromObject(obj));
        ItemResponse<T> response = await container.Container.CreateItemAsync(obj, documentPartitionKey);
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.OK });
    }


    private static (string id, PartitionKey) ExtractDocumentQueryValues(ContainerResponse container, JObject document)
    {
        string containerPartitionKey = container.Resource.PartitionKeyPath.TrimStart('/');

        JToken partitionKeyValue = document[containerPartitionKey] ??
            throw new ArgumentException($"Could not find partitionkey {containerPartitionKey} on object");

        PartitionKey partitionKey = container.Container.Id == ApplicationDataContainerName ?
            new PartitionKey(partitionKeyValue.Value<int>()) :
                new PartitionKey(partitionKeyValue.ToString());

        string documentId = document["id"]?.ToString() ?? throw new ArgumentException("Could not find id on object");

        return (documentId, partitionKey);
    }

    private static QueryDefinition CreateQueryDefinitionForIdentifiers(List<string> identifiers)
    {
        if (identifiers.Count == 0)
        {
            return new("SELECT * FROM c");
        }

        StringBuilder queryBuilder = new("SELECT * FROM c WHERE c.id IN (");

        for (int i = 0; i < identifiers.Count; i++)
        {
            string paramName = $"@id{i}";
            queryBuilder.Append(paramName);

            if (i < identifiers.Count - 1)
            {
                queryBuilder.Append(", ");
            }
        }
        queryBuilder.Append(')');

        QueryDefinition queryDefinition = new(queryBuilder.ToString());

        // Add parameters to the query definition
        for (int i = 0; i < identifiers.Count; i++)
        {
            queryDefinition.WithParameter($"@id{i}", identifiers[i]);
        }

        return queryDefinition;
    }

    private static async Task EnsureItemIsQueryableAsync<T>(
        Container container,
        string documentId,
        PartitionKey partitionKey,
        int maxAttempts = 10,
        int delayMilliseconds = 500)
        where T : class
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                ItemResponse<T> readResponse = await container.ReadItemAsync<T>(documentId, partitionKey);
                if (readResponse.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Item not yet visible
            }

            await Task.Delay(delayMilliseconds);
        }

        throw new TimeoutException($"Item with ID {documentId} was not queryable after {maxAttempts} attempts.");
    }
}
