using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.CosmosDb;
public sealed class CosmosDbTestDatabase : IAsyncDisposable
{
    private const string DatabaseId = "giapsearch";
    private const string ApplicationDataContainerName = "application-data";
    private readonly CosmosClient _cosmosClient;

    public CosmosDbTestDatabase(RepositoryOptions options)
    {
        _cosmosClient = new(
            accountEndpoint: options.EndpointUri,
            authKeyOrResourceToken: options.PrimaryKey,
            new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway,
                // Consider in future if necessary - requires better control of docker emulator args, performance cost
                // ConsistencyLevel = ConsistencyLevel.Strong
            });
    }

    public async ValueTask DisposeAsync()
    {
        using (_cosmosClient)
        {
            await DeleteDatabase();
        }
    }

    public async Task StartAsync()
    {
        DatabaseResponse db = await CreateDatabase(_cosmosClient);
        await CreateAllContainers(db);
    }

    public async Task ClearDatabaseAsync()
    {
        DatabaseResponse response = await CreateDatabase(_cosmosClient);
        List<ContainerResponse> containers = await CreateAllContainers(response);

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

    public async Task DeleteDatabase() => await _cosmosClient!.GetDatabase(DatabaseId).DeleteAsync();

    public async Task<IEnumerable<T>> ReadManyAsync<T>() where T : class
    {
        ContainerResponse targetContainer = await GetTargetContainer<T>();

        List<T> output = [];
        QueryDefinition queryDefinition = new($"SELECT * FROM c");
        FeedIterator<T> iterator = targetContainer.Container.GetItemQueryIterator<T>(queryDefinition);
        while (iterator.HasMoreResults)
        {
            FeedResponse<T> item = await iterator.ReadNextAsync();
            output.AddRange(item.Resource);
        }
        return output;
    }

    public async Task<IEnumerable<T>> ReadManyAsync<T>(IEnumerable<string> identifiers) where T : class
    {
        IEnumerable<T> results = await ReadManyAsync<T>();
        IEnumerable<string> resultIdentifiers = results.Select(ExtractDocumentIdFromDto);


        List<string> missingIdentifiers = identifiers.Except(resultIdentifiers).ToList();
        if (missingIdentifiers.Any())
        {
            throw new ArgumentException($"Unable to find identifier(s): {string.Join(", ", missingIdentifiers)}");
        }

        IEnumerable<T> matchingResults = results.Where(t => identifiers.Contains(ExtractDocumentIdFromDto(t)));

        return matchingResults;

    }

    public async Task WriteItemAsync<T>(T item) where T : class
    {
        ContainerResponse targetContainer = await GetTargetContainer<T>();

        (string documentId, PartitionKey documentPartitionKey) = ExtractDocumentQueryValues(targetContainer, JObject.FromObject(item));

        await CreateItemInternalAsync(item, targetContainer);

        await EnsureItemIsQueryableAsync<T>(
                targetContainer.Container,
                documentId,
                documentPartitionKey);
    }


    public async Task WriteManyAsync<T>(IEnumerable<T> items) where T : class
    {
        ContainerResponse container = await GetTargetContainer<T>();

        await Task.WhenAll(
            items.Select(
                (item) => CreateItemInternalAsync(item, container)));

        await Task.WhenAll(
            items.Select(
                (item) =>
                {
                    (string id, PartitionKey key) = ExtractDocumentQueryValues(container, JObject.FromObject(item));

                    return EnsureItemIsQueryableAsync<T>(
                        container,
                        id,
                        key);
                }
                ));
    }

    private async Task<ContainerResponse> GetTargetContainer<T>()
    {
        Dictionary<Type, string> typeToContainerNameMap = new()
        {
            {  typeof(NewsArticleDto), "news" },
            {  typeof(UserDto), "users" }
        };

        DatabaseResponse db = await CreateDatabase(_cosmosClient);
        List<ContainerResponse> containers = await CreateAllContainers(db);

        typeToContainerNameMap.TryGetValue(typeof(T), out string? containerName);

        string targetContainer = containerName ?? ApplicationDataContainerName; // Default back to ApplicationData
        return containers.Single((container) => container.Container.Id == targetContainer);
    }

    private static string ExtractDocumentIdFromDto<T>(T obj) where T : class
        => JObject.FromObject(obj)
                .GetValue("id")?
                .ToString() ?? throw new ArgumentException("Unable to find id on written document");


    private static async Task<DatabaseResponse> CreateDatabase(CosmosClient client)
    {
        // TODO guard for failed db creation
        return await client!.CreateDatabaseIfNotExistsAsync(DatabaseId);
    }

    private static async Task<List<ContainerResponse>> CreateAllContainers(Database database)
    {
        ArgumentNullException.ThrowIfNull(database);
        List<ContainerResponse> containerResponses = [];

        // TODO hardcoded Container -> PartitionKey relationships
        Dictionary<string, string> containers = new()
        {
            {  ApplicationDataContainerName, "/DOCTYPE" },
            {  "news", "/id" },
            {  "users", "/id" },
        };

        foreach (KeyValuePair<string, string> container in containers)
        {
            ContainerResponse applicationData = await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
            {
                Id = container.Key,
                PartitionKeyPath = container.Value,
            });
            containerResponses.Add(applicationData);
        }

        return containerResponses;
    }

    private static async Task CreateItemInternalAsync<T>(T obj, ContainerResponse container) where T : class
    {
        (string id, PartitionKey documentPartitionKey) = ExtractDocumentQueryValues(container, JObject.FromObject(obj));
        ItemResponse<T> response = await container.Container.CreateItemAsync(obj, documentPartitionKey);
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.OK });
    }

    private static string ExtractPartitionKeyForContainer(ContainerResponse container)
    {
        string partitionKeyPath = container.Resource.PartitionKeyPath; // e.g., "/application-data"
        string partitionKeyProperty = partitionKeyPath.TrimStart('/');
        return partitionKeyProperty;
    }

    private static (string id, PartitionKey) ExtractDocumentQueryValues(ContainerResponse container, JObject document)
    {
        string containerPartitionKey = ExtractPartitionKeyForContainer(container);

        string id = document["id"]?.ToString() ?? throw new ArgumentException("Could not find id on object");

        JToken partitionKeyValue = document[containerPartitionKey] ??
            throw new ArgumentException($"Could not find partitionkey {containerPartitionKey} on object");

        PartitionKey partitionKey = container.Container.Id == ApplicationDataContainerName ?
            new PartitionKey(partitionKeyValue.Value<int>()) :
                new PartitionKey(partitionKeyValue.ToString());

        return (id, partitionKey);
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
