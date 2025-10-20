namespace DfE.GIAP.SharedTests.Infrastructure.CosmosDb.Options;
public record CosmosDbOptions
{
    private readonly IReadOnlyList<CosmosDbDatabaseOptions> _databases;
    public Uri Uri { get; }
    public string Key { get; }

    public IReadOnlyList<string> DatabaseNames =>
        _databases.Select(t => t.DatabaseName)
            .ToList()
            .AsReadOnly();

    public IReadOnlyList<CosmosDbContainerOptions> Containers =>
        _databases.SelectMany(t => t.Containers)
            .ToList()
            .AsReadOnly();

    public CosmosDbOptions(string uri, string? key, IEnumerable<CosmosDbDatabaseOptions> databases)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uri);
        if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri? result))
        {
            throw new ArgumentException("Invalid URI format", nameof(uri));
        }
        Uri = result;

        Key = key ?? string.Empty;

        _databases = databases?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(databases));

        if (_databases.Count == 0)
        {
            throw new ArgumentException("At least one database must be specified", nameof(databases)); ;
        }
    }

    public CosmosDbDatabaseOptions GetDatabaseOptionsByName(string databaseName) =>
        _databases.First((options)
            => options.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
}
