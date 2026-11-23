using System.Globalization;
using System.Text.Json.Nodes;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// The old pipeline's <c>giapdataprocessing</c> container, which has the rows each data file's quality checks
/// rejected. It's only read.
/// </summary>
/// <remarks>
/// The old pipeline writes a data file's rejected rows to <c>&lt;folder&gt;/report/giap_exceptions_&lt;run ID&gt;.csv</c>
/// with pandas, and only if there are any. The file has the rows themselves, with a first column for pandas' index
/// and missing values as <c>nan</c>.
/// </remarks>
public sealed class LegacyProcessingStorage(BlobContainerClient containerClient)
{
    public const string ConfigurationKey = "Verification:LegacyProcessingStorage";

    private static readonly HashSet<string> _missingValues = ["", "nan", "None", "NaN"];

    /// <returns>The rejected rows, or <see langword="null"/> if there's no file for the run.</returns>
    public async Task<IReadOnlyList<JsonObject>?> ReadRejectedRowsAsync(IngestFolder folder, string runId, CancellationToken cancellationToken = default)
    {
        BlobClient blob = containerClient.GetBlobClient($"{ReportPrefix(folder)}{runId}.csv");

        try
        {
            return await ReadRejectedRowsAsync(blob, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// Finds the rejected rows file written while a data file was processed, from when it was last modified.
    /// </summary>
    /// <returns>The rejected rows, or <see langword="null"/> unless exactly one file was written in that time.</returns>
    public async Task<IReadOnlyList<JsonObject>?> FindRejectedRowsAsync(
        IngestFolder folder,
        DateTime processingStarted,
        DateTime processingEnded,
        CancellationToken cancellationToken = default)
    {
        List<string> candidates = [];
        await foreach (BlobItem item in containerClient.GetBlobsAsync(prefix: ReportPrefix(folder), cancellationToken: cancellationToken))
        {
            if (item.Properties.LastModified is { } lastModified
                && lastModified.UtcDateTime >= processingStarted
                && lastModified.UtcDateTime <= processingEnded)
            {
                candidates.Add(item.Name);
            }
        }

        return candidates.Count == 1
            ? await ReadRejectedRowsAsync(containerClient.GetBlobClient(candidates[0]), cancellationToken)
            : null;
    }

    public static async Task<IReadOnlyList<JsonObject>> ParseRejectedRowsAsync(Stream content, CancellationToken cancellationToken = default)
    {
        using StreamReader streamReader = new(content);
        using CsvReader csv = new(streamReader, CultureInfo.InvariantCulture);

        List<JsonObject> rows = [];
        if (!await csv.ReadAsync() || !csv.ReadHeader() || csv.HeaderRecord is not { } header)
        {
            return rows;
        }

        // The first column is pandas' index, which has no name
        int firstColumn = header.Length > 0 && header[0].Length == 0 ? 1 : 0;

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            JsonObject row = [];
            for (int i = firstColumn; i < header.Length; i++)
            {
                string? value = csv.GetField(i);
                row[header[i]] = value is null || _missingValues.Contains(value) ? null : value;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static async Task<IReadOnlyList<JsonObject>> ReadRejectedRowsAsync(BlobClient blob, CancellationToken cancellationToken)
    {
        await using Stream content = await blob.OpenReadAsync(cancellationToken: cancellationToken);

        return await ParseRejectedRowsAsync(content, cancellationToken);
    }

    private static string ReportPrefix(IngestFolder folder) => $"{folder.Name}/report/giap_exceptions_";
}
