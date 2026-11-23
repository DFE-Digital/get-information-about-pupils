using System.Text.Json.Nodes;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Reports;
using DfE.GIAP.Verification.Sources;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Verification.Checks;

/// <summary>
/// Compares each data file's results, and the rows each pipeline rejected from it.
/// </summary>
/// <remarks>
/// Children looked after files aren't compared, as the old pipeline only split them into prepared downloads.
/// </remarks>
public sealed class DataFileChecks(
    IDbContextFactory<GiapDbContext> dbContextFactory,
    LegacyFileResults legacyFileResults,
    LegacyProcessingStorage legacyProcessingStorage)
{
    public const string FileResultsComparison = "file-results";
    public const string RejectedRowsComparison = "rejected-rows";

    /// <param name="dataFiles">The data files to compare, or <see langword="null"/> for every one either pipeline has processed.</param>
    /// <param name="legacyRunIds">The old pipeline's run ID for a data file name, if it's known.</param>
    public async Task CompareAsync(
        IReadOnlyList<ScopedDataFile>? dataFiles,
        IReadOnlyDictionary<string, string> legacyRunIds,
        VerificationReport report,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<string, LegacyFileResult> legacyResults =
            await legacyFileResults.ReadLatestAsync(dataFiles?.Select(f => f.DataFileName).ToArray(), cancellationToken);

        Dictionary<string, IngestRun> latestRuns = await ReadLatestRunsAsync(dataFiles, cancellationToken);

        // Every data file name either pipeline has a result for, without folders, as the old pipeline names them
        IEnumerable<string> fileNames = dataFiles?.Select(f => Path.GetFileName(f.DataFileName))
            ?? latestRuns.Keys.Concat(legacyResults.Keys);

        foreach (string fileName in fileNames.Distinct().Order(StringComparer.Ordinal))
        {
            IngestRun? run = latestRuns.GetValueOrDefault(fileName);
            LegacyFileResult? legacyResult = legacyResults.GetValueOrDefault(fileName);
            IngestDataFileAttributes? attributes = DataFileScope.TryParse(fileName);

            if (attributes?.Dataset is IngestDataset.ChildrenLookedAfterKs2 or IngestDataset.ChildrenLookedAfterKs4)
            {
                continue;
            }

            CompareResults(fileName, run, legacyResult, report);

            if (attributes is not null && run is not null)
            {
                await CompareRejectedRowsAsync(fileName, attributes, run, legacyResult, legacyRunIds.GetValueOrDefault(fileName), report, cancellationToken);
            }
        }
    }

    internal static void CompareResults(string fileName, IngestRun? run, LegacyFileResult? legacyResult, VerificationReport report)
    {
        bool succeeded = run?.Status == IngestRunStatus.Succeeded;

        Dictionary<string, JsonNode?> newFields = new() { ["Succeeded"] = succeeded };
        Dictionary<string, JsonNode?> oldFields = new() { ["Succeeded"] = legacyResult is not null };

        // The counts only mean the same thing if both pipelines wrote the data file
        if (succeeded && legacyResult is not null)
        {
            newFields["Number Of Source Records"] = run!.RecordsRead;
            newFields["Number Of Failed QC Records"] = run.RecordsRejected;
            newFields["Number Of Passed QC Records"] = run.RecordsRead - run.RecordsRejected;
            newFields["Number Of Unique Records"] = run.RecordsWritten;

            oldFields["Number Of Source Records"] = legacyResult.SourceRecords;
            oldFields["Number Of Failed QC Records"] = legacyResult.FailedQcRecords;
            oldFields["Number Of Passed QC Records"] = legacyResult.PassedQcRecords;
            oldFields["Number Of Unique Records"] = legacyResult.UniqueRecords;
        }

        foreach (Difference difference in RecordComparer.Compare(
            FileResultsComparison,
            fileName,
            new ComparedRecord(fileName, newFields),
            new ComparedRecord(fileName, oldFields)))
        {
            report.Add(difference);
        }

        report.AddCompared(FileResultsComparison, 1);
    }

    private async Task CompareRejectedRowsAsync(
        string fileName,
        IngestDataFileAttributes attributes,
        IngestRun run,
        LegacyFileResult? legacyResult,
        string? legacyRunId,
        VerificationReport report,
        CancellationToken cancellationToken)
    {
        IngestFolder folder = IngestFolder.All.First(f => f.Datasets.Contains(attributes.Dataset));
        string dataset = attributes.Dataset.ToString();

        IReadOnlyList<JsonObject>? oldRows = await ReadLegacyRejectedRowsAsync(folder, legacyResult, legacyRunId, cancellationToken);
        if (oldRows is null)
        {
            report.Add(new Difference(RejectedRowsComparison, DifferenceKinds.NotCompared, dataset, Field: null, fileName));
            return;
        }

        List<JsonObject> newRows;
        await using (GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            newRows = [.. (await dbContext.IngestRejectedRows.AsNoTracking()
                    .Where(r => r.RunId == run.Id)
                    .OrderBy(r => r.RowNumber)
                    .Select(r => r.Data)
                    .ToListAsync(cancellationToken))
                .Select(d => JsonNode.Parse(d)!.AsObject())];
        }

        string? identityColumn = DataFileScope.KeyColumns.TryGetValue(attributes.Dataset, out (RecordContainer, string KeyColumn) key)
            ? key.KeyColumn
            : null;

        foreach (Difference difference in RecordComparer.CompareRows(RejectedRowsComparison, fileName, dataset, newRows, oldRows, identityColumn))
        {
            report.Add(difference);
        }

        report.AddCompared(RejectedRowsComparison, Math.Max(newRows.Count, oldRows.Count));
    }

    /// <returns>The old pipeline's rejected rows, or <see langword="null"/> if they can't be found.</returns>
    private async Task<IReadOnlyList<JsonObject>?> ReadLegacyRejectedRowsAsync(
        IngestFolder folder,
        LegacyFileResult? legacyResult,
        string? legacyRunId,
        CancellationToken cancellationToken)
    {
        if (legacyRunId is not null)
        {
            // The old pipeline only writes the file if there are rejected rows
            return await legacyProcessingStorage.ReadRejectedRowsAsync(folder, legacyRunId, cancellationToken) ?? [];
        }

        if (legacyResult is null)
        {
            return null;
        }

        if (ValueComparer.Canonicalise(legacyResult.FailedQcRecords) == ValueComparer.Canonicalise(0))
        {
            return [];
        }

        return legacyResult is { Started: { } started, Ended: { } ended }
            ? await legacyProcessingStorage.FindRejectedRowsAsync(folder, started, ended, cancellationToken)
            : null;
    }

    private async Task<Dictionary<string, IngestRun>> ReadLatestRunsAsync(IReadOnlyList<ScopedDataFile>? dataFiles, CancellationToken cancellationToken)
    {
        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<IngestRun> runs = dbContext.IngestRuns.AsNoTracking();
        if (dataFiles is not null)
        {
            string[] filePaths = [.. dataFiles.Select(f => f.FilePath).Distinct()];
            runs = runs.Where(r => filePaths.Contains(r.FilePath));
        }

        HashSet<(string, string)>? scope = dataFiles?.Select(f => (f.FilePath, f.DataFileName)).ToHashSet();

        return (await runs.ToListAsync(cancellationToken))
            .Where(r => scope is null || scope.Contains((r.FilePath, r.DataFileName)))
            .GroupBy(r => Path.GetFileName(r.DataFileName))
            .ToDictionary(g => g.Key, g => g.MaxBy(r => r.StartedAt)!);
    }
}
