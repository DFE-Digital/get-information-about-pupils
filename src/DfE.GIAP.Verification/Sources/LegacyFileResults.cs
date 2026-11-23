using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// The document the old pipeline writes to the <c>reference</c> container when it has processed a data file.
/// </summary>
/// <remarks>
/// The old pipeline's <c>ReferenceDataGetter.get_ref_data_extended</c>. It's only written once a data file has been
/// written to Cosmos DB, so there's none for a data file that failed. Its <c>id</c> is when it was written, as
/// <c>yyyyMMddHHmmss</c>.
/// </remarks>
public sealed record LegacyFileResult(
    string Id,
    string FileName,
    JsonNode? SourceRecords,
    JsonNode? FailedQcRecords,
    JsonNode? PassedQcRecords,
    JsonNode? UniqueRecords,
    DateTime? Started,
    DateTime? Ended)
{
    public static LegacyFileResult? FromDocument(JsonObject document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document["File Name"]?.ToString() is not { } fileName || document["id"]?.ToString() is not { } id)
        {
            return null;
        }

        return new LegacyFileResult(
            id,
            Path.GetFileName(fileName),
            document["Number Of Source Records"]?.DeepClone(),
            document["Number Of Failed QC Records"]?.DeepClone(),
            document["Number Of Passed QC Records"]?.DeepClone(),
            document["Number Of Unique Records"]?.DeepClone(),
            ParseDateTime(document["Start DateTime"]),
            ParseDateTime(document["End DateTime"]));
    }

    // Databricks runs in UTC
    private static DateTime? ParseDateTime(JsonNode? value) =>
        DateTime.TryParseExact(
            value?.ToString(),
            "yyyy-MM-dd-HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out DateTime dateTime)
            ? dateTime
            : null;
}

/// <summary>
/// Reads the <see cref="LegacyFileResult"/>s from the old pipeline's <c>reference</c> container. The container also
/// has the GIAS reference data, which isn't read.
/// </summary>
public sealed class LegacyFileResults(CosmosQueries queries)
{
    /// <returns>The latest result for each data file name, without any folders.</returns>
    public async Task<IReadOnlyDictionary<string, LegacyFileResult>> ReadLatestAsync(
        IReadOnlyCollection<string>? fileNames,
        CancellationToken cancellationToken = default)
    {
        QueryDefinition query = new("SELECT * FROM c WHERE IS_DEFINED(c[\"File Name\"])");

        IEnumerable<LegacyFileResult> results = (await queries.QueryAsync(query, cancellationToken))
            .OfType<JsonObject>()
            .Select(LegacyFileResult.FromDocument)
            .OfType<LegacyFileResult>();

        if (fileNames is not null)
        {
            // The file name can have the folders it was in in the zip, so it's matched after reading
            HashSet<string> names = [.. fileNames.Select(Path.GetFileName).OfType<string>()];
            results = results.Where(r => names.Contains(r.FileName));
        }

        return results
            .GroupBy(r => r.FileName)
            .ToDictionary(g => g.Key, g => g.MaxBy(r => r.Id, StringComparer.Ordinal)!);
    }
}
