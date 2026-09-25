using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;

public sealed class GiapCosmosDbFixture : BaseCosmosDbFixture
{
    public string DatabaseName => "giapsearch";

    protected override IEnumerable<CosmosDbDatabaseOptions> Databases => [
        new CosmosDbDatabaseOptions(
            databaseName: DatabaseName,
            containers: [
                new("application-data", "/DOCTYPE", PartitionKeyType.Integer ),
                new("news", "/id" ),
                new("users", "/id" ),
                new("mypupils", "/id")
            ]
        )
    ];
}
