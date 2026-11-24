using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Checks;
using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Reports;
using DfE.GIAP.Verification.Sources;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Verification;

public enum CompareScope
{
    /// <summary>
    /// Only what's in particular zips: their pupils and learners, data files and prepared downloads.
    /// </summary>
    Keys,

    /// <summary>
    /// Everything on either side.
    /// </summary>
    Full
}

public enum Check
{
    Records,
    DataFiles,
    Downloads
}

/// <param name="Files">The zips' ingest file paths, e.g. <c>ks2/NPD_KS2_2025_U.zip</c>; only for <see cref="CompareScope.Keys"/>.</param>
/// <param name="LegacyRunIds">The old pipeline's run ID for a data file name, to find its rejected rows.</param>
/// <param name="Detail">Whether to write every difference, with its values, as well as the summary.</param>
/// <param name="MaxRequestUnits">The most request units a second to use in Cosmos DB, or <see langword="null"/> for no limit.</param>
/// <param name="OutputDirectory">A local folder to write the report to, as well as the <c>verification</c> container.</param>
/// <param name="RulesPath">The accepted differences; <c>accepted-differences.json</c> next to the tool if <see langword="null"/>.</param>
public sealed record CompareOptions(
    CompareScope Scope,
    IReadOnlyList<string> Files,
    IReadOnlyDictionary<string, string> LegacyRunIds,
    IReadOnlySet<Check> Checks,
    bool Detail,
    int BatchSize,
    int Concurrency,
    double? MaxRequestUnits,
    string? OutputDirectory,
    string? RulesPath);

/// <summary>
/// Compares what the new pipeline wrote with what the old pipeline wrote, and writes a report of the differences.
/// </summary>
public static class CompareCommand
{
    public const int CleanExitCode = 0;
    public const int DifferencesExitCode = 1;
    public const int ErrorExitCode = 2;

    /// <summary>
    /// The container in the ingest storage account the reports are written to.
    /// </summary>
    public const string ReportsContainerName = "verification";

    /// <summary>
    /// A connection string for the old pipeline's downloads storage account, to use instead of <c>BlobStorageOptions</c>.
    /// </summary>
    public const string LegacyDownloadsStorageKey = "Verification:LegacyDownloadsStorage";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) }
    };

    /// <param name="output">Where the summary is written; it has no values, so it's safe to show anywhere.</param>
    /// <param name="log">Where progress is written.</param>
    /// <returns>One of the exit codes.</returns>
    public static async Task<int> RunAsync(
        CompareOptions options,
        IConfiguration configuration,
        TextWriter output,
        TextWriter log,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);

        if (options.Scope == CompareScope.Keys && options.Files.Count == 0)
        {
            await log.WriteLineAsync("A keys comparison needs at least one --file.");
            return ErrorExitCode;
        }

        if (options.Scope == CompareScope.Full && options.Files.Count > 0)
        {
            await log.WriteLineAsync("A full comparison compares everything, so it can't have --file.");
            return ErrorExitCode;
        }

        DateTime startedAt = DateTime.UtcNow;
        string reportFolder = $"reports/{startedAt.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture)}";

        AcceptedDifferences acceptedDifferences = AcceptedDifferences.Load(
            options.RulesPath ?? Path.Combine(AppContext.BaseDirectory, AcceptedDifferences.DefaultFileName));

        string ingestStorageConnectionString = GetRequired(configuration.GetConnectionString(IngestStorage.ConnectionName), $"ConnectionStrings:{IngestStorage.ConnectionName}");

        ServiceCollection services = new();
        services
            .AddGiapDbContext(GetRequired(configuration.GetConnectionString(GiapDbContext.ConnectionName), $"ConnectionStrings:{GiapDbContext.ConnectionName}"))
            .AddIngestDependencies(ingestStorageConnectionString);
        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IDbContextFactory<GiapDbContext> dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<GiapDbContext>>();

        using CosmosClient cosmosClient = CreateCosmosClient(configuration);
        Database database = cosmosClient.GetDatabase(GetRequired(configuration["RepositoryOptions:DatabaseId"], "RepositoryOptions:DatabaseId"));
        RequestUnitThrottle throttle = new(options.MaxRequestUnits, TimeProvider.System);
        CheckOptions checkOptions = new(options.BatchSize, options.Concurrency);

        string detailPath = Path.GetTempFileName();
        try
        {
            bool isClean;
            string markdown;
            string json;

            await using (StreamWriter? detail = options.Detail ? new StreamWriter(detailPath) : null)
            using (VerificationReport report = new(acceptedDifferences, detail))
            {
                DataFileScope? scope = null;
                if (options.Scope == CompareScope.Keys)
                {
                    await log.WriteLineAsync($"Reading the keys in {options.Files.Count} files…");
                    scope = await DataFileScope.ReadAsync(options.Files, dbContextFactory, serviceProvider.GetRequiredService<IngestStorage>(), cancellationToken);
                }

                if (options.Checks.Contains(Check.Records))
                {
                    foreach (RecordContainer container in RecordContainer.All)
                    {
                        RecordsCheck check = new(
                            container,
                            new PostgresRecordSource(dbContextFactory, container),
                            new CosmosRecordSource(new CosmosQueries(GetContainer(database, configuration, container.Name), throttle), container),
                            checkOptions);

                        if (scope is null)
                        {
                            await log.WriteLineAsync($"Comparing every record in {container.Name}…");
                            await check.CompareAllAsync(report, cancellationToken);
                        }
                        else if (scope.Keys[container].Count > 0)
                        {
                            await log.WriteLineAsync($"Comparing {scope.Keys[container].Count:N0} records in {container.Name}…");
                            await check.CompareKeysAsync([.. scope.Keys[container]], report, cancellationToken);
                        }
                    }
                }

                if (options.Checks.Contains(Check.DataFiles))
                {
                    await log.WriteLineAsync("Comparing data file results and rejected rows…");
                    DataFileChecks check = new(
                        dbContextFactory,
                        new LegacyFileResults(new CosmosQueries(GetContainer(database, configuration, "reference"), throttle)),
                        new LegacyProcessingStorage(CreateLegacyProcessingContainer(configuration)));
                    await check.CompareAsync(scope?.DataFiles, options.LegacyRunIds, report, cancellationToken);
                }

                if (options.Checks.Contains(Check.Downloads))
                {
                    await log.WriteLineAsync("Comparing prepared downloads…");
                    DownloadsCheck check = new(
                        new BlobContainerClient(ingestStorageConnectionString, PreparedDownloadPublisher.ContainerName),
                        CreateLegacyDownloadsContainer(configuration),
                        checkOptions);
                    await check.CompareAsync(scope?.FirstSeenAt, report, cancellationToken);
                }

                isClean = report.IsClean;
                markdown = report.ToMarkdown(Title(options));
                json = JsonSerializer.Serialize(
                    new
                    {
                        startedAt,
                        finishedAt = DateTime.UtcNow,
                        options.Scope,
                        options.Files,
                        isClean,
                        compared = report.ComparedCounts,
                        groups = report.Groups
                    },
                    _jsonOptions);
            }

            await output.WriteAsync(markdown);

            Dictionary<string, string> files = new()
            {
                ["summary.md"] = markdown,
                ["summary.json"] = json
            };
            await WriteReportAsync(ingestStorageConnectionString, reportFolder, files, options.Detail ? detailPath : null, cancellationToken);
            await log.WriteLineAsync($"The report is in the {ReportsContainerName} container, in {reportFolder}/.");

            if (options.OutputDirectory is not null)
            {
                Directory.CreateDirectory(options.OutputDirectory);
                foreach ((string name, string content) in files)
                {
                    await File.WriteAllTextAsync(Path.Combine(options.OutputDirectory, name), content, cancellationToken);
                }

                if (options.Detail)
                {
                    File.Copy(detailPath, Path.Combine(options.OutputDirectory, "details.csv"), overwrite: true);
                }
            }

            return isClean ? CleanExitCode : DifferencesExitCode;
        }
        finally
        {
            File.Delete(detailPath);
        }
    }

    private static async Task WriteReportAsync(
        string ingestStorageConnectionString,
        string reportFolder,
        Dictionary<string, string> files,
        string? detailPath,
        CancellationToken cancellationToken)
    {
        BlobContainerClient container = new(ingestStorageConnectionString, ReportsContainerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        foreach ((string name, string content) in files)
        {
            await container.GetBlobClient($"{reportFolder}/{name}").UploadAsync(BinaryData.FromString(content), cancellationToken);
        }

        if (detailPath is not null)
        {
            await container.GetBlobClient($"{reportFolder}/details.csv").UploadAsync(detailPath, cancellationToken);
        }
    }

    private static string Title(CompareOptions options) =>
        options.Scope == CompareScope.Full
            ? "Verification of everything"
            : $"Verification of {string.Join(", ", options.Files.Select(f => $"`{f}`"))}";

    internal static CosmosClient CreateCosmosClient(IConfiguration configuration) =>
        new(
            GetRequired(configuration["RepositoryOptions:EndpointUri"], "RepositoryOptions:EndpointUri"),
            GetRequired(configuration["RepositoryOptions:PrimaryKey"], "RepositoryOptions:PrimaryKey"),
            new CosmosClientOptions
            {
                ConnectionMode = configuration.GetValue("RepositoryOptions:ConnectionMode", ConnectionMode.Direct),
                // Wait as long as Cosmos DB says to when it throttles a request, rather than failing
                MaxRetryAttemptsOnRateLimitedRequests = 30,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromMinutes(5)
            });

    internal static Container GetContainer(Database database, IConfiguration configuration, string key) =>
        database.GetContainer(GetRequired(configuration[$"RepositoryOptions:Containers:{key}:ContainerName"], $"RepositoryOptions:Containers:{key}:ContainerName"));

    // The web app's BlobStorageOptions in deployed environments; a connection string can be given instead, e.g. for Azurite
    private static BlobContainerClient CreateLegacyDownloadsContainer(IConfiguration configuration)
    {
        if (configuration[LegacyDownloadsStorageKey] is { Length: > 0 } connectionString)
        {
            return new BlobContainerClient(connectionString, "giapdownloads");
        }

        BlobStorageOptions options = configuration.GetSection(BlobStorageOptions.SectionName).Get<BlobStorageOptions>()
            ?? throw new InvalidOperationException($"{BlobStorageOptions.SectionName} isn't configured.");

        return new BlobContainerClient(options.ConnectionString, options.ContainerName);
    }

    // A SAS URL for the container in deployed environments; a connection string locally, e.g. for Azurite
    private static BlobContainerClient CreateLegacyProcessingContainer(IConfiguration configuration)
    {
        string value = GetRequired(configuration[LegacyProcessingStorage.ConfigurationKey], LegacyProcessingStorage.ConfigurationKey);

        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps
            ? new BlobContainerClient(uri)
            : new BlobContainerClient(value, "giapdataprocessing");
    }

    internal static string GetRequired(string? value, string key) =>
        string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"{key} isn't configured.") : value;
}
