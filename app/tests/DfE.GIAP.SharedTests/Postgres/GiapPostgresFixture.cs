using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.SharedTests.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DfE.GIAP.SharedTests.Postgres;

/// <summary>
/// Starts a PostgreSQL container and applies the <see cref="GiapDbContext"/> migrations to it,
/// so tests run against the same schema as a migrated environment.
/// <para>
/// Apply with <c>IClassFixture</c> or <c>ICollectionFixture</c>; xUnit calls
/// <see cref="InitializeAsync"/> before the first test that uses it.
/// </para>
/// </summary>
public sealed class GiapPostgresFixture : IAsyncLifetime
{
    private readonly PostgresServer _server = new();

    /// <summary>
    /// Connection string for the migrated database. Only valid once <see cref="InitializeAsync"/> has completed.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _server.StartAsync();

        ConnectionString = _server.ConnectionString;

        await using GiapDbContext dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public GiapDbContext CreateDbContext() => GiapDbContext.Create(ConnectionString);

    public async Task DisposeAsync() => await _server.DisposeAsync();
}
