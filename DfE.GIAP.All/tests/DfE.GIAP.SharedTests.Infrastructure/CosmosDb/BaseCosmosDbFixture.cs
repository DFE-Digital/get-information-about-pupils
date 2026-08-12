using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;

namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;

// Note: Limitation that XUnit does not have native dependency resolution for fixtures. This prevents us creating constructors with dependencies e.g Options. So clients must override Databases with their options and apply ICollectionFixture<DerivedFixture>
// IAsyncLifetime is called by XUnit when applied through a fixture

public abstract class BaseCosmosDbFixture : IAsyncLifetime
{
    private readonly CosmosDbEmulator _emulator = new();
    private IReadOnlyDictionary<string, CosmosDbDatabaseClient>? _dbClients = null;

    /// <summary>
    /// The databases (and their containers) to create in the emulator before tests run.
    /// </summary>
    protected abstract IEnumerable<CosmosDbDatabaseOptions> Databases { get; }

    /// <summary>
    /// Connection details for the emulator this fixture started. Only valid once
    /// <see cref="InitializeAsync"/> has completed - the host port is not known before then.
    /// </summary>
    public CosmosDbConnection Connection { get; private set; } = CosmosDbConnection.NotStarted;

    public async Task InitializeAsync()
    {
        await _emulator.StartAsync();

        Connection = new CosmosDbConnection(_emulator.Endpoint, _emulator.Key);

        CosmosDbOptions options = new(_emulator.Endpoint, _emulator.Key, Databases);

        Dictionary<string, CosmosDbDatabaseClient> clients = [];

        foreach (string databaseName in options.DatabaseNames)
        {
            CosmosDbDatabaseClient dbClient =
                new(
                    options.Uri,
                    options.Key,
                    options.GetDatabaseOptionsByName(databaseName));

            await dbClient.CreateAsync();
            await dbClient.ClearDatabaseAsync();
            await OnInitialiseAsync(dbClient);

            clients.Add(databaseName, dbClient);
        }

        _dbClients = clients;
    }

    public Task InvokeAsync(string databaseName, Func<CosmosDbDatabaseClient, Task> handler)
        => handler(
            GetDatabaseClientByName(databaseName));

    public Task<T> InvokeAsync<T>(string databaseName, Func<CosmosDbDatabaseClient, Task<T>> handler)
        => handler(
            GetDatabaseClientByName(databaseName));

    public virtual async Task DisposeAsync()
    {
        foreach (KeyValuePair<string, CosmosDbDatabaseClient> item in _dbClients?.ToList() ?? [])
        {
            await OnDisposeAsync(item.Value);
            await item.Value.DisposeAsync();
        }

        await _emulator.DisposeAsync();
    }

    protected virtual Task OnInitialiseAsync(CosmosDbDatabaseClient client) => Task.CompletedTask;
    protected virtual Task OnDisposeAsync(CosmosDbDatabaseClient client) => Task.CompletedTask;

    private CosmosDbDatabaseClient GetDatabaseClientByName(string databaseName)
    {
        Guard.ThrowIfNullOrWhiteSpace(databaseName, nameof(databaseName));

        CosmosDbDatabaseClient client =
            _dbClients.Single(t => t.Key.Equals(databaseName, StringComparison.Ordinal))
                .Value;

        return client;
    }
}
