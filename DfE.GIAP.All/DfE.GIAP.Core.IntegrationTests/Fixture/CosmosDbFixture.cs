using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using DfE.GIAP.Core.SharedTests;

namespace DfE.GIAP.Core.IntegrationTests.Fixture;
public sealed class CosmosDbFixture : IAsyncLifetime
{
    public CosmosDbTestDatabase Database { get; private set; }
    public async Task InitializeAsync()
    {
        RepositoryOptions options = RepositoryOptionsFactory.LocalCosmosDbEmulator();
        Database = new(options);
        await Database.StartAsync();
        await Database.ClearDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (Database != null)
        {
            await Database.DisposeAsync();
        }
    }
}
