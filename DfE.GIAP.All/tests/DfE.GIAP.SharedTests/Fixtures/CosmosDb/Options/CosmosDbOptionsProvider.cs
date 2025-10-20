namespace DfE.GIAP.SharedTests.Fixtures.CosmosDb.Options;

public static class CosmosDbOptionsProvider
{
    public static CosmosDbOptions DefaultLocalOptions() => new(
        uri: "https://localhost:8081",
        key: "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
        databases: DatabasesOptions);

    private static IEnumerable<CosmosDbDatabaseOptions> DatabasesOptions =>
    [
        new CosmosDbDatabaseOptions(
            databaseName: "giapsearch",
            containers: [
                new CosmosDbContainerOptions("application-data", "/DOCTYPE"),
                new CosmosDbContainerOptions("news", "/id"),
                new CosmosDbContainerOptions("users", "/id"),
                new CosmosDbContainerOptions("mypupils", "/id")
                /*new CosmosDbContainerOptions("further-education", "/ULN"),
                new CosmosDbContainerOptions("pupil-noskill", "/PupilMatchingRef"),
                new CosmosDbContainerOptions("pupil-premium-v2", "/PupilMatchingRef"),
                new CosmosDbContainerOptions("reference", "/DOCTYPE")*/
            ]
        )
    ];
}
