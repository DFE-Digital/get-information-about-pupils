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
                new("mypupils", "/id"),
                // The old pipeline's containers, which the verification tool compares with Postgres
                new("pupil-noskill", "/PupilMatchingRef"),
                new("pupil-premium-v2", "/PupilMatchingRef"),
                new("further-education-v2", "/ULN"),
                new("reference", "/DOCTYPE")
            ]
        )
    ];
}
