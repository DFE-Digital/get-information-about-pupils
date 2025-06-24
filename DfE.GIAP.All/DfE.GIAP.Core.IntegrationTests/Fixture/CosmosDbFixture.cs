using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;

namespace DfE.GIAP.Core.IntegrationTests.Fixture;
public sealed class CosmosDbFixture : IAsyncLifetime
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CosmosDbTestDatabase Database { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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
