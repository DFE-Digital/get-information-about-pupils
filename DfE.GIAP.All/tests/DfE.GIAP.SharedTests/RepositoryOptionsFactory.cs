using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;

namespace DfE.GIAP.SharedTests;
public static class RepositoryOptionsFactory
{
#pragma warning disable S1075 // URIs should not be hardcoded
    public static RepositoryOptions LocalCosmosDbEmulator() => new()
    {
        ConnectionMode = 1,
        EndpointUri = "https://localhost:8081/",
        DatabaseId = "giapsearch",
        PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
    };
#pragma warning restore S1075 // URIs should not be hardcoded
}
