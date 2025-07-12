using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace DfE.GIAP.Core.IntegrationTests.Fixture;
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
                    string id = item.id.ToString();
                    PartitionKey partitionKey = new((int)item.DOCTYPE); // TODO currently hardcoded to application-data partitionKey.

                    deleteTasks.Add(container.Container.DeleteItemAsync<dynamic>(id, partitionKey));
                }
            }
            await Task.WhenAll(deleteTasks);
            await container.Container.ReadContainerAsync();
        }
    }

    public async Task DeleteDatabase() => await _cosmosClient!.GetDatabase(DatabaseId).DeleteAsync();

    public async Task<IEnumerable<T>> ReadManyAsync<T>() where T : class
    {
        ContainerResponse targetContainer = await GetApplicationDataContainer();

        int partitionKey = ExtractApplicationDataPartitionKey<T>();

        List<T> output = [];
        QueryDefinition queryDefinition = new($"SELECT * FROM c WHERE c.DOCTYPE = {partitionKey}");
        FeedIterator<T> iterator = targetContainer.Container.GetItemQueryIterator<T>(queryDefinition);
        while (iterator.HasMoreResults)
        {
            FeedResponse<T> item = await iterator.ReadNextAsync();
            output.AddRange(item.Resource);
        }
        return output;
    }
    public async Task<IEnumerable<T?>> ReadManyAsync<T>(IEnumerable<string> identifiers) where T : class
    {
        ContainerResponse targetContainer = await GetApplicationDataContainer();
        int partitionKey = ExtractApplicationDataPartitionKey<T>();
        IEnumerable<Task<T?>> readTasks = identifiers.Select(async (id) =>
        {
            try
            {
                ItemResponse<T> response = await targetContainer.Container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Item not found, return null TODO consider NullObject
                return null;
            }
        });

        return await Task.WhenAll(readTasks);
    }

    public async Task WriteItemAsync<T>(T item) where T : class
    {
        ContainerResponse targetContainer = await GetApplicationDataContainer();

        PartitionKey documentPartitionKey = new((item as dynamic).DOCTYPE);

        await CreateItemInternalAsync(item, targetContainer);

        string documentId = ExtractDocumentId(item);

        await EnsureItemIsQueryableAsync<T>(
                targetContainer.Container,
                documentId,
                documentPartitionKey);
    }


    public async Task WriteManyAsync<T>(IEnumerable<T> items) where T : class
    {
        ContainerResponse container = await GetApplicationDataContainer();

        await Task.WhenAll(
            items.Select(
                (item) => CreateItemInternalAsync(item, container)));

        await Task.WhenAll(
            items.Select(
                (item) => EnsureItemIsQueryableAsync<T>(
                    container,
                    ExtractDocumentId(item),
                    ExtractPartitionKey(item))));
    }

    private async Task<ContainerResponse> GetApplicationDataContainer()
    {
        DatabaseResponse db = await CreateDatabase(_cosmosClient);
        List<ContainerResponse> containers = await CreateAllContainers(db);
        ContainerResponse targetContainer = containers.Single((container) => container.Container.Id == ApplicationDataContainerName);
        return targetContainer;
    }

    private static int ExtractApplicationDataPartitionKey<T>() where T : class // DOCTYPE
    {
        // TODO Temp to query without point-reading ability (on id) - PartitionKey value needs to be passed as part of query
        // TODO make partition key or config - configurable not pinned to application-data-container e.g
        Dictionary<string, int> typeToPartitionKeyMap = new()
        {
            { nameof(NewsArticleDto), 7 },
            { nameof(ContentDto), 20 }
        };

        return typeToPartitionKeyMap[typeof(T).Name];
    }

    private static string ExtractDocumentId<T>(T obj) where T : class
        => JObject.FromObject(obj)
                .GetValue("id")?
                .ToString() ?? throw new ArgumentException("Unable to find id on written document");

    private static PartitionKey ExtractPartitionKey<T>(T obj) where T : class => new((obj as dynamic).DOCTYPE);

    private static async Task<DatabaseResponse> CreateDatabase(CosmosClient client)
    {
        // TODO guard for failed db creation
        return await client!.CreateDatabaseIfNotExistsAsync(DatabaseId);
    }

    private static async Task<List<ContainerResponse>> CreateAllContainers(Database database)
    {
        ArgumentNullException.ThrowIfNull(database);
        List<ContainerResponse> containerResponses = [];
        ContainerResponse response = await database.CreateContainerIfNotExistsAsync(new ContainerProperties()
        {
            Id = ApplicationDataContainerName,
            PartitionKeyPath = "/DOCTYPE", // TODO hardcoded AND assumes there is a single partitionkey per logical partition
        });
        containerResponses.Add(response);
        return containerResponses;
    }

    private static async Task CreateItemInternalAsync<T>(T obj, Container container) where T : class
    {
        PartitionKey partitionKey = new((obj as dynamic).DOCTYPE);
        ItemResponse<T> response = await container.CreateItemAsync(obj, partitionKey);
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.OK });
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
