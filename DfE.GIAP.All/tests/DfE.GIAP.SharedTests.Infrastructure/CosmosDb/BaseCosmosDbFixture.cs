using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
using Xunit;

namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

// Note: Limitation that XUnit does not have native dependency resolution for fixtures. This prevents us creating constructors with dependencies e.g Options. So clients must override Options with their options and apply ICollectionFixture<DerivedFixture>
// IAsyncLifetime is called by XUnit when applied through a fixture

public abstract class BaseCosmosDbFixture : IAsyncLifetime
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    protected abstract CosmosDbOptions Options { get; }
    private IReadOnlyDictionary<string, CosmosDbDatabaseClient>? _dbClients = null;
    public CosmosDbDatabaseClient Database => _dbClients.Single(t => t.Key.Equals("giapsearch")).Value;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public async Task InitializeAsync()
    {
        Dictionary<string, CosmosDbDatabaseClient> clients = [];

        ArgumentNullException.ThrowIfNull(Options);

        foreach (string databaseName in Options.DatabaseNames)
        {
            CosmosDbDatabaseClient dbClient =
                new(
                    Options.Uri,
                    Options.Key,
                    Options.GetDatabaseOptionsByName(databaseName));

            await dbClient.CreateAsync();
            await dbClient.ClearDatabaseAsync();
            await OnInitialiseAsync(dbClient);

            clients.Add(databaseName, dbClient);
        }

        _dbClients = clients;
    }

    public virtual async Task DisposeAsync()
    {
        if (Database is not null)
        {
            await OnDisposeAsync(Database);
            await Database.DisposeAsync();
        }
    }

    protected virtual Task OnInitialiseAsync(CosmosDbDatabaseClient client) => Task.CompletedTask;
    protected virtual Task OnDisposeAsync(CosmosDbDatabaseClient client) => Task.CompletedTask;

    private CosmosDbDatabaseClient GetDatabaseClientByName(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        CosmosDbDatabaseClient client =
            _dbClients.Single(t => t.Key.Equals(databaseName, StringComparison.Ordinal))
                .Value;

        return client;
    }
}
