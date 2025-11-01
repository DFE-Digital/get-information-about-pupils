using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;

namespace DfE.GIAP.Core.IntegrationTests.TestHarness;
public sealed class CosmosDbFixture : BaseCosmosDbFixture
{
    protected override CosmosDbOptions Options => new(
        databaseOptions: [
            new CosmosDbDatabaseOptions(
                databaseName: "giapsearch",
                containers: [
                    new("application-data", "/DOCTYPE" ),
                    new("news", "/id" ),
                    new("users", "/id" ),
                    new("mypupils", "/id")
                ]
            )
        ]);
}
