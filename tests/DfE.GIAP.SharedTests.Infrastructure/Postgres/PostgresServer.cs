using Testcontainers.PostgreSql;

namespace DfE.GIAP.SharedTests.Infrastructure.Postgres;

/// <summary>
/// Owns the lifetime of a PostgreSQL server container, started on demand via Testcontainers.
/// The container publishes PostgreSQL on an ephemeral host port, so <see cref="ConnectionString"/>
/// is only valid once <see cref="StartAsync"/> has completed.
/// </summary>
public sealed class PostgresServer : IAsyncDisposable
{
    // Matches the major version deployed by terraform/application (postgres_server_version).
    private const string ImageName = "postgres:18";

    private readonly PostgreSqlContainer _container;

    public PostgresServer()
    {
        _container = new PostgreSqlBuilder(ImageName).Build();
    }

    /// <summary>
    /// Npgsql connection string for the running server's default database, including the mapped host port.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
