using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Core.IntegrationTests.Common.Infrastructure.Postgres;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class GiapDbContextTests(GiapPostgresFixture postgres)
{
    [Fact]
    public async Task Migrations_AreAllApplied()
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();

        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        Assert.Empty(pendingMigrations);
    }

    [Fact]
    public async Task Model_HasNoChangesMissingAMigration()
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();

        // Fails when the model has changed without a migration; run `just ef migrations add <MigrationName>`.
        Assert.False(dbContext.Database.HasPendingModelChanges());
    }
}
