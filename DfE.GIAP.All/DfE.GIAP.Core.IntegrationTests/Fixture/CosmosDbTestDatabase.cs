using System.Net;
using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

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
            authKeyOrResourceToken: options.PrimaryKey);
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
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c");
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

    public async Task WriteAsync<T>(T obj) where T : class// TODO batch options, TODO targetcontainer 
    {
        DatabaseResponse db = await CreateDatabase(_cosmosClient);
        List<ContainerResponse> containers = await CreateAllContainers(db);

        ContainerResponse targetContainer =
            containers.Single(
                (container) => container.Container.Id == ApplicationDataContainerName);

        ItemResponse<T> response = await targetContainer.Container.UpsertItemAsync(obj, new PartitionKey((obj as dynamic).DocumentType));

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.OK });
    }

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
}
