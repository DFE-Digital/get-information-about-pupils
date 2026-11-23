using System.Text.Json.Nodes;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Verification.Checks;
using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Reports;
using DfE.GIAP.Verification.Sources;

namespace DfE.GIAP.Verification.Tests.Checks;

public class DataFileChecksTests
{
    private const string FileName = "NPD_KS2_2025_U_KtS.txt";

    [Fact]
    public void CompareResults_SameCounts_IsClean()
    {
        using VerificationReport report = new(new AcceptedDifferences([]));

        DataFileChecks.CompareResults(FileName, Run(IngestRunStatus.Succeeded), LegacyResult(failed: 1), report);

        Assert.Empty(report.Groups);
        Assert.Equal(1, report.ComparedCounts[DataFileChecks.FileResultsComparison]);
    }

    [Fact]
    public void CompareResults_DifferentCounts_ReportsEachCount()
    {
        using VerificationReport report = new(new AcceptedDifferences([]));

        DataFileChecks.CompareResults(FileName, Run(IngestRunStatus.Succeeded), LegacyResult(failed: 2), report);

        Assert.Equal(
            ["Number Of Failed QC Records", "Number Of Passed QC Records"],
            report.Groups.Select(g => g.Field).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void CompareResults_FailedInOnePipeline_OnlyReportsTheStatus()
    {
        using VerificationReport report = new(new AcceptedDifferences([]));

        DataFileChecks.CompareResults(FileName, Run(IngestRunStatus.Failed), LegacyResult(failed: 1), report);

        DifferenceGroup group = Assert.Single(report.Groups);
        Assert.Equal("Succeeded", group.Field);
        Assert.Equal(DifferenceKinds.ValueDifferent, group.Kind);
    }

    private static IngestRun Run(IngestRunStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            FilePath = "ks2/NPD_KS2_2025_U.zip",
            Attempt = 1,
            DataFileName = FileName,
            RecordsRead = 10,
            RecordsRejected = 1,
            RecordsWritten = 8,
            Status = status,
            StartedAt = DateTime.UtcNow
        };

    private static LegacyFileResult LegacyResult(int failed) =>
        new(
            "20250102030405",
            FileName,
            SourceRecords: JsonValue.Create(10),
            FailedQcRecords: JsonValue.Create(failed),
            PassedQcRecords: JsonValue.Create(10 - failed),
            UniqueRecords: JsonValue.Create(8),
            Started: null,
            Ended: null);
}
