namespace DfE.GIAP.SharedTests.Fixtures.CosmosDb.Options;

public record CosmosDbDatabaseOptions
{
    public string DatabaseName { get; }
    public IReadOnlyList<CosmosDbContainerOptions> Containers { get; }

    public CosmosDbDatabaseOptions(string databaseName, IEnumerable<CosmosDbContainerOptions>? containers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        DatabaseName = databaseName;
        Containers = (containers ?? []).ToList().AsReadOnly();
    }
}
