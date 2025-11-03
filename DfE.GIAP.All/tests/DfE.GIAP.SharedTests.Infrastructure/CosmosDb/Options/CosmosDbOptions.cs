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

    public CosmosDbOptions(IEnumerable<CosmosDbDatabaseOptions> databaseOptions)
        : this(
              "https://localhost:8081",
              "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
              databaseOptions)
    {

    }

    public CosmosDbOptions(string uri, string? key, IEnumerable<CosmosDbDatabaseOptions> databaseOptions)
    {
        Guard.ThrowIfNullOrWhiteSpace(uri, nameof(uri));
        if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri? result))
        {
            throw new ArgumentException("Invalid URI format", nameof(uri));
        }
        Uri = result;

        Key = key ?? string.Empty;

        _databases = databaseOptions?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(databaseOptions));

        if (_databases.Count == 0)
        {
            throw new ArgumentException("At least one database must be specified", nameof(databaseOptions)); ;
        }
    }

    public CosmosDbDatabaseOptions GetDatabaseOptionsByName(string databaseName) =>
        _databases.First((options)
            => options.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
}
