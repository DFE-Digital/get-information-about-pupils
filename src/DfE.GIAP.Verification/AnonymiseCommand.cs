using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Anonymisation;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Verification;

/// <param name="ManifestPath">The manifest of zips to anonymise.</param>
/// <param name="KeyVaultName">The production <c>app</c> Key Vault to read the secrets from, or <see langword="null"/> to use configuration.</param>
/// <param name="ListColumns">Whether to describe the zips' data files rather than anonymise them.</param>
/// <param name="Overwrite">Whether to replace anonymised zips that are already there.</param>
/// <param name="ClassificationPath">The classification; <c>anonymisation.json</c> next to the tool if <see langword="null"/>.</param>
public sealed record AnonymiseOptions(
    string ManifestPath,
    string? KeyVaultName,
    bool ListColumns,
    bool Overwrite,
    string? ClassificationPath);

/// <summary>
/// Anonymises real zips from the old pipeline's production storage into the test environment's <c>verification</c>
/// container, so both pipelines can be run on them in test.
/// </summary>
public static class AnonymiseCommand
{
    public static async Task<int> RunAsync(
        AnonymiseOptions options,
        IConfiguration configuration,
        TextWriter output,
        TextWriter log,
        CancellationToken cancellationToken = default)
    {
        AnonymisationManifest manifest = AnonymisationManifest.Load(options.ManifestPath);
        AnonymisationClassification classification = options.ClassificationPath is null
            ? AnonymisationClassification.LoadShipped()
            : AnonymisationClassification.Load(options.ClassificationPath);

        AnonymisationStorage storage = new(configuration, options.KeyVaultName);
        BlobContainerClient source = await storage.GetSourceAsync(cancellationToken);

        if (options.ListColumns)
        {
            foreach (ManifestFile file in manifest.Files)
            {
                await output.WriteLineAsync($"## `{file.Source}`");
                await output.WriteLineAsync();
                await using Stream zip = await source.GetBlobClient(file.Source).OpenReadAsync(cancellationToken: cancellationToken);
                ZipProfiler.Profile(zip, file.Folder, classification, output);
                await output.WriteLineAsync();
            }

            return CompareCommand.CleanExitCode;
        }

        (BlobContainerClient destination, byte[] key) = await storage.GetDestinationAsync(cancellationToken);
        using ValueAnonymiser values = new(key, classification.PmrFormats, NameDictionary.LoadShipped());
        ZipAnonymiser anonymiser = new(new DataFileAnonymiser(values), classification);

        foreach ((ManifestFile file, int index) in manifest.Files.Select((f, i) => (f, i + 1)))
        {
            await log.WriteLineAsync($"[{index}/{manifest.Files.Count}] Anonymising {file.Source} to {file.Target}…");

            await using Stream zip = await source.GetBlobClient(file.Source).OpenReadAsync(cancellationToken: cancellationToken);
            await using BlockBlobWriteStream anonymised = new(destination.GetBlockBlobClient($"{AnonymisationStorage.FilesFolder}/{file.Target}"));

            ZipAnonymisationResult result;
            try
            {
                result = anonymiser.Anonymise(zip, anonymised, file.Folder);
            }
            catch (AnonymisationException ex)
            {
                await log.WriteLineAsync($"{file.Source} can't be anonymised, so nothing was written for it: {ex.Message}");
                return CompareCommand.ErrorExitCode;
            }

            bool written = await anonymised.CommitAsync(options.Overwrite, cancellationToken);
            foreach ((string dataFile, int rows) in result.Rows)
            {
                await log.WriteLineAsync($"  {dataFile}: {rows:N0} rows");
            }

            await log.WriteLineAsync(written ? "  Written." : "  It's already there, so it was left as it was; use --overwrite to replace it.");
        }

        await using (BlockBlobWriteStream manifestBlob = new(destination.GetBlockBlobClient($"{AnonymisationStorage.FilesFolder}/{AnonymisationManifest.FileName}")))
        {
            manifestBlob.Write(Encoding.UTF8.GetBytes(manifest.ToJson()));
            await manifestBlob.CommitAsync(overwrite: true, cancellationToken);
        }

        await log.WriteLineAsync($"The anonymised files and their manifest are in the {AnonymisationStorage.FilesFolder}/ folder of the {AnonymisationStorage.DestinationContainerName} container.");
        return CompareCommand.CleanExitCode;
    }

    /// <summary>
    /// Lists the zips in the old pipeline's <c>raw/</c> and <c>processed/</c> folders, to choose the manifest from.
    /// Only their names, sizes and dates are listed.
    /// </summary>
    public static async Task<int> ListSourcesAsync(
        IConfiguration configuration,
        string? keyVaultName,
        IReadOnlyCollection<string> folderNames,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        BlobContainerClient source = await new AnonymisationStorage(configuration, keyVaultName).GetSourceAsync(cancellationToken);
        IEnumerable<IngestFolder> folders = folderNames.Count == 0 ? IngestFolder.All : folderNames.Select(IngestFolder.Get);

        await output.WriteLineAsync("| Path | Size | Last modified |");
        await output.WriteLineAsync("| --- | --: | --- |");

        foreach (IngestFolder folder in folders)
        {
            foreach (string subfolder in new[] { "raw", "processed" })
            {
                await foreach (BlobItem blob in source.GetBlobsAsync(prefix: $"{AnonymisationStorage.LegacyFolderName(folder)}/{subfolder}/", cancellationToken: cancellationToken))
                {
                    await output.WriteLineAsync($"| `{blob.Name}` | {blob.Properties.ContentLength:N0} | {blob.Properties.LastModified:yyyy-MM-dd HH:mm} |");
                }
            }
        }

        return CompareCommand.CleanExitCode;
    }
}
