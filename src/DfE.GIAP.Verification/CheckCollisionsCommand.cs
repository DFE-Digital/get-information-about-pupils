using System.IO.Compression;
using System.Text.Json;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Anonymisation;
using DfE.GIAP.Verification.Sources;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Verification;

/// <summary>
/// Finds the pupils and learners in the anonymised zips whose fake PMR or ULN is already in the test environment, on
/// either side. Fake PMRs are in a reserved range, but fake ULNs can't be, so a few will match junk already in test.
/// The verification workflow's reset deletes them, from <c>files/collisions.json</c>, before the zips are replayed.
/// </summary>
/// <remarks>
/// Run it in the test environment's Worker pod, once the zips have been anonymised and before they're first replayed:
/// after that, the replayed pupils are in test too.
/// </remarks>
public static class CheckCollisionsCommand
{
    public const string CollisionsFileName = "collisions.json";

    private const int BatchSize = 1000;

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <returns><see cref="CompareCommand.CleanExitCode"/> if there are no collisions, or <see cref="CompareCommand.DifferencesExitCode"/> if there are.</returns>
    public static async Task<int> RunAsync(IConfiguration configuration, TextWriter output, TextWriter log, CancellationToken cancellationToken = default)
    {
        string ingestStorageConnectionString = CompareCommand.GetRequired(configuration.GetConnectionString(IngestStorage.ConnectionName), $"ConnectionStrings:{IngestStorage.ConnectionName}");
        BlobContainerClient files = new(ingestStorageConnectionString, AnonymisationStorage.DestinationContainerName);

        AnonymisationManifest manifest = AnonymisationManifest.Parse(
            (await files.GetBlobClient($"{AnonymisationStorage.FilesFolder}/{AnonymisationManifest.FileName}").DownloadContentAsync(cancellationToken)).Value.Content.ToString());

        Dictionary<RecordContainer, HashSet<string>> keys = RecordContainer.All.ToDictionary(c => c, _ => new HashSet<string>(StringComparer.Ordinal));
        foreach (ManifestFile file in manifest.Files)
        {
            await log.WriteLineAsync($"Reading the keys in {file.Target}…");
            await ReadKeysAsync(files.GetBlobClient($"{AnonymisationStorage.FilesFolder}/{file.Target}"), keys, cancellationToken);
        }

        ServiceCollection services = new();
        services.AddGiapDbContext(CompareCommand.GetRequired(configuration.GetConnectionString(GiapDbContext.ConnectionName), $"ConnectionStrings:{GiapDbContext.ConnectionName}"));
        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IDbContextFactory<GiapDbContext> dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<GiapDbContext>>();

        using CosmosClient cosmosClient = CompareCommand.CreateCosmosClient(configuration);
        Database database = cosmosClient.GetDatabase(CompareCommand.GetRequired(configuration["RepositoryOptions:DatabaseId"], "RepositoryOptions:DatabaseId"));
        RequestUnitThrottle throttle = new(null, TimeProvider.System);

        Dictionary<string, string[]> collisions = [];
        await output.WriteLineAsync("| Container | Keys in the files | Already in Postgres | Already in Cosmos DB |");
        await output.WriteLineAsync("| --- | --: | --: | --: |");

        foreach (RecordContainer container in RecordContainer.All)
        {
            await log.WriteLineAsync($"Looking for {keys[container].Count:N0} keys in {container.Name}…");

            PostgresRecordSource postgres = new(dbContextFactory, container);
            CosmosRecordSource cosmos = new(new CosmosQueries(CompareCommand.GetContainer(database, configuration, container.Name), throttle), container);

            HashSet<string> inPostgres = new(StringComparer.Ordinal);
            HashSet<string> inCosmos = new(StringComparer.Ordinal);
            foreach (string[] batch in keys[container].Chunk(BatchSize))
            {
                inPostgres.UnionWith(await postgres.FindExistingAsync(batch, cancellationToken));
                inCosmos.UnionWith(await cosmos.FindExistingAsync(batch, cancellationToken));
            }

            await output.WriteLineAsync($"| {container.Name} | {keys[container].Count:N0} | {inPostgres.Count:N0} | {inCosmos.Count:N0} |");

            string[] colliding = [.. inPostgres.Union(inCosmos).Order(StringComparer.Ordinal)];
            if (colliding.Length > 0)
            {
                collisions[container.Name] = colliding;
            }
        }

        await files.GetBlobClient($"{AnonymisationStorage.FilesFolder}/{CollisionsFileName}")
            .UploadAsync(BinaryData.FromObjectAsJson(collisions, _jsonOptions), overwrite: true, cancellationToken);
        await log.WriteLineAsync($"The keys that collide are in {AnonymisationStorage.FilesFolder}/{CollisionsFileName}.");

        return collisions.Count == 0 ? CompareCommand.CleanExitCode : CompareCommand.DifferencesExitCode;
    }

    private static async Task ReadKeysAsync(BlobClient blob, Dictionary<RecordContainer, HashSet<string>> keys, CancellationToken cancellationToken)
    {
        await using Stream zip = await blob.OpenReadAsync(cancellationToken: cancellationToken);
        using ZipArchive archive = new(zip, ZipArchiveMode.Read);

        foreach (ZipArchiveEntry entry in archive.Entries.Where(IngestFileProcessor.IsDataFile))
        {
            if (DataFileScope.TryParse(entry.Name) is { } attributes
                && DataFileScope.KeyColumns.TryGetValue(attributes.Dataset, out (RecordContainer Container, string KeyColumn) key))
            {
                await using Stream content = entry.Open();
                await DataFileScope.ReadKeysAsync(content, key.KeyColumn, keys[key.Container], cancellationToken);
            }
        }
    }
}
