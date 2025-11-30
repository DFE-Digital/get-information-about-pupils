using System.Net;
using System.Text;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Xunit;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;


// TODO composition alternative CosmosDbContext - contains the Client creation and operations, which is passed into each TestDatabase
public class CosmosDbDatabaseClient : IAsyncDisposable
{
    private readonly string _databaseName;
    private readonly IReadOnlyList<CosmosDbContainerOptions> _containerOptions;
    private readonly CosmosClient _cosmosClient;

    public CosmosDbDatabaseClient(
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

    public async Task CreateAsync()
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
                    (string id, PartitionKey pk) = ExtractDocumentQueryValues(container, itemObject, _containerOptions);
                    deleteTasks.Add(container.Container.DeleteItemAsync<dynamic>(id, pk));
                }
            }
            await Task.WhenAll(deleteTasks);
            await container.Container.ReadContainerAsync();
        }
    }

    public async Task DeleteDatabase() => await _cosmosClient!.GetDatabase(_databaseName).DeleteAsync();

    public async Task<List<TDto>> ReadManyAsync<TDto>(string containerName, IEnumerable<string>? identifiers = null) where TDto : class
    {
        Guard.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));
        ContainerResponse targetContainer = await GetContainerByName(containerName);

        // Build the query string
        QueryDefinition query = CreateQueryDefinitionForIdentifiers(identifiers?.ToList() ?? []);

        List<TDto> results = [];
        FeedIterator<TDto>? iterator = targetContainer.Container.GetItemQueryIterator<TDto>(query);

        while (iterator.HasMoreResults)
        {
            FeedResponse<TDto>? response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        // NOTE does not guard if an identifier is not found
        return results;
    }


    public async Task WriteItemAsync<TDto>(string containerName, TDto value) where TDto : class
    {
        ContainerResponse targetContainer = await GetContainerByName(containerName);

        (string documentId, PartitionKey documentPartitionKey) = ExtractDocumentQueryValues(targetContainer, JObject.FromObject(value), _containerOptions);

        await CreateItemInternalAsync(value, targetContainer);

        await EnsureItemIsQueryableAsync<TDto>(
                targetContainer.Container,
                documentId,
                documentPartitionKey);
    }


    public async Task WriteManyAsync<TDto>(string containerName, IEnumerable<TDto> items) where TDto : class
    {
        ContainerResponse targetContainer = await GetContainerByName(containerName);

        List<TDto> createItems = items?.ToList() ?? [];

        foreach (TDto item in createItems)
        {
            await CreateItemInternalAsync(item, targetContainer);

            (string id, PartitionKey key) = ExtractDocumentQueryValues(targetContainer, JObject.FromObject(item), _containerOptions);

            await EnsureItemIsQueryableAsync<TDto>(
                targetContainer,
                id,
                key);
        }
    }

    private async Task<ContainerResponse> GetContainerByName(string containerName)
    {
        DatabaseResponse databaseResponse = await _cosmosClient!.CreateDatabaseIfNotExistsAsync(_databaseName);
        List<ContainerResponse> containerResponses = await CreateAllContainersIfNotExistAsync(databaseResponse);
        return containerResponses.Single(t => t.Container.Id.Equals(containerName, StringComparison.Ordinal));
    }

    private async Task<List<ContainerResponse>> CreateAllContainersIfNotExistAsync(Database database)
    {
        Guard.ThrowIfNull(database, nameof(database));
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

    private async Task CreateItemInternalAsync<T>(T obj, ContainerResponse container) where T : class
    {
        (string _, PartitionKey documentPartitionKey) = ExtractDocumentQueryValues(container, JObject.FromObject(obj), _containerOptions);
        ItemResponse<T> response = await container.Container.CreateItemAsync(obj, documentPartitionKey);
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.OK });
    }


    private static (string id, PartitionKey) ExtractDocumentQueryValues(ContainerResponse container, JObject document, IEnumerable<CosmosDbContainerOptions> options)
    {
        string containerPartitionKey = container.Resource.PartitionKeyPath.TrimStart('/');

        JToken partitionKeyValue = document[containerPartitionKey] ??
            throw new ArgumentException($"Could not find partitionkey {containerPartitionKey} on object");

        CosmosDbContainerOptions targetOptions =
            options.Single(
                (options) => options.ContainerName.Equals(container.Container.Id));

        PartitionKey partitionKey = targetOptions.PartitionKeyType switch
        {
            PartitionKeyType.Integer => new(partitionKeyValue.Value<int>()),
            _ => new(partitionKeyValue.ToString())
        };

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
