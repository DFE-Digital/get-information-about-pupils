namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
public record CosmosDbDatabaseOptions
{
    public string DatabaseName { get; }
    public IReadOnlyList<CosmosDbContainerOptions> Containers { get; }

    public CosmosDbDatabaseOptions(string databaseName, IEnumerable<CosmosDbContainerOptions>? containers)
    {
        Guard.ThrowIfNullOrWhiteSpace(databaseName, nameof(databaseName));
        DatabaseName = databaseName;
        Containers = (containers ?? []).ToList().AsReadOnly();
    }
}
