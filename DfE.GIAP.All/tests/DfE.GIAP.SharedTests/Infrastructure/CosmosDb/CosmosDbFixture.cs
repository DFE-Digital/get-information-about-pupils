using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
using Xunit;


namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
public sealed class CosmosDbFixture : IAsyncLifetime
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DefaultCosmosDbTestDatabase Database { get; private set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public async Task InitializeAsync()
    {
        CosmosDbOptions options = CosmosDbOptionsProvider.DefaultLocalOptions();

        Database = new DefaultCosmosDbTestDatabase(
            options.Uri,
            options.Key,
            options.GetDatabaseOptionsByName(databaseName: "giapsearch"));

        await Database.InitialiseAsync();
        await Database.ClearDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (Database is not null)
        {
            await Database.DisposeAsync();
        }
    }
}
