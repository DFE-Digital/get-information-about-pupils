using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.PupilData;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Core.IntegrationTests.PupilData;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class PupilPremiumDataWriterTests(GiapPostgresFixture postgres) : IAsyncLifetime
{
    private const string AcademicYear = "2024/2025";

    private readonly PupilPremiumDataWriter _writer = new(new GiapPostgresDbContextFactory(postgres));

    public async Task InitializeAsync()
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        await dbContext.IngestRejectedRows.ExecuteDeleteAsync();
        await dbContext.IngestRuns.ExecuteDeleteAsync();
        await dbContext.IngestFiles.ExecuteDeleteAsync();
        await dbContext.PupilPremiumPupils.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task WriteFileAsync_WritesPupils()
    {
        PupilPremiumFileRow row = CreateRow(1, "P1", data: """{"NCYear": "5"}""");

        PupilPremiumFileWriteResult result = await WriteFileAsync(row);

        Assert.Equal(new PupilPremiumFileWriteResult(RecordsWritten: 1, RecordsRemoved: 0), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equivalent(
            new PupilPremiumPupil
            {
                PupilMatchingRef = "P1",
                Upn = "A123456789012",
                LaCode = 201,
                EstablishmentNumber = 3614,
                Urn = 100000,
                Surname = "Smith",
                Forename = "Alex",
                MiddleNames = "Jo",
                Sex = "M",
                Dob = new DateOnly(2015, 2, 1),
                SourceFlag = "C",
                Data = """[{"NCYear": "5"}]"""
            },
            await dbContext.PupilPremiumPupils.SingleAsync());
    }

    [Fact]
    public async Task WriteFileAsync_PupilWithMoreThanOneRow_CombinesRowsInFileOrderWithFieldsFromFirstRow()
    {
        PupilPremiumFileWriteResult result = await WriteFileAsync(
            CreateRow(1, "P1", surname: "First", data: """{"URN": "1"}"""),
            CreateRow(2, "P2", data: """{"URN": "2"}"""),
            CreateRow(3, "P1", surname: "Second", data: """{"URN": "3"}"""));

        Assert.Equal(2, result.RecordsWritten);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        PupilPremiumPupil pupil = await dbContext.PupilPremiumPupils.SingleAsync(p => p.PupilMatchingRef == "P1");
        Assert.Equal("First", pupil.Surname);
        Assert.Equal("""[{"URN": "1"}, {"URN": "3"}]""", pupil.Data);
    }

    [Theory]
    [InlineData(new[] { "C" }, "C")]
    [InlineData(new[] { "C", "C" }, "C")]
    [InlineData(new[] { "A", "C" }, "Both")]
    [InlineData(new[] { "C", null, "C" }, "C")]
    [InlineData(new[] { "A", "C", "X" }, "Both")]
    [InlineData(new string?[] { null }, null)]
    public async Task WriteFileAsync_SourceFlag_IsTheSourceOrBothIfTheRowsDiffer(string?[] sources, string? expected)
    {
        await WriteFileAsync([.. sources.Select((s, i) => CreateRow(i + 1, "P1", source: s))]);

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(expected, (await dbContext.PupilPremiumPupils.SingleAsync()).SourceFlag);
    }

    [Fact]
    public async Task WriteFileAsync_PupilMissingFromNewFile_IsRemoved()
    {
        await WriteFileAsync(CreateRow(1, "P1"), CreateRow(2, "P2"));

        PupilPremiumFileWriteResult result = await WriteFileAsync(CreateRow(1, "P2", surname: "New"), CreateRow(2, "P3"));

        Assert.Equal(new PupilPremiumFileWriteResult(RecordsWritten: 2, RecordsRemoved: 1), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(
            [("P2", "New"), ("P3", "Smith")],
            (await dbContext.PupilPremiumPupils.OrderBy(p => p.PupilMatchingRef).ToListAsync()).Select(p => (p.PupilMatchingRef, p.Surname)));
    }

    [Theory]
    [InlineData("2024/2025", IngestVersion.Unamended, "2024/2025", IngestVersion.Unamended)]
    [InlineData("2024/2025", IngestVersion.Unamended, "2024/2025", IngestVersion.Final)]
    [InlineData("2024/2025", IngestVersion.Final, "2024/2025", IngestVersion.Final)]
    [InlineData("2023/2024", IngestVersion.Final, "2024/2025", IngestVersion.Unamended)]
    public async Task WriteFileAsync_FileAtLeastAsRecentAsLastSucceededRun_IsWritten(
        string storedAcademicYear,
        IngestVersion storedVersion,
        string academicYear,
        IngestVersion ingestVersion)
    {
        await WriteFileAsync(CreateRow(1, "P1"));
        await AddRunAsync(storedAcademicYear, storedVersion, IngestRunStatus.Succeeded);

        await _writer.WriteFileAsync(academicYear, ingestVersion, ToAsync(CreateRow(1, "P2")));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("P2", (await dbContext.PupilPremiumPupils.SingleAsync()).PupilMatchingRef);
    }

    [Theory]
    [InlineData("2024/2025", IngestVersion.Final, "2024/2025", IngestVersion.Unamended)]
    [InlineData("2024/2025", IngestVersion.Unamended, "2023/2024", IngestVersion.Final)]
    public async Task WriteFileAsync_FileOlderThanLastSucceededRun_ThrowsAndLeavesStoredPupils(
        string storedAcademicYear,
        IngestVersion storedVersion,
        string academicYear,
        IngestVersion ingestVersion)
    {
        await WriteFileAsync(CreateRow(1, "P1"));
        await AddRunAsync(storedAcademicYear, storedVersion, IngestRunStatus.Succeeded);

        PupilPremiumFileVersionException exception = await Assert.ThrowsAsync<PupilPremiumFileVersionException>(
            () => _writer.WriteFileAsync(academicYear, ingestVersion, ToAsync(CreateRow(1, "P2"))));

        Assert.Equal((storedAcademicYear, storedVersion), (exception.StoredAcademicYear, exception.StoredIngestVersion));
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("P1", (await dbContext.PupilPremiumPupils.SingleAsync()).PupilMatchingRef);
    }

    [Fact]
    public async Task WriteFileAsync_ComparesWithTheLastSucceededRunOnly()
    {
        await AddRunAsync("2025/2026", IngestVersion.Final, IngestRunStatus.Succeeded);
        await AddRunAsync("2023/2024", IngestVersion.Unamended, IngestRunStatus.Succeeded);
        await AddRunAsync("2025/2026", IngestVersion.Final, IngestRunStatus.Failed);
        await AddRunAsync("2025/2026", IngestVersion.Final, IngestRunStatus.Succeeded, IngestDataset.Ks2);

        await WriteFileAsync(CreateRow(1, "P1"));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(1, await dbContext.PupilPremiumPupils.CountAsync());
    }

    [Fact]
    public async Task WriteFileAsync_NoRows_ThrowsAndLeavesStoredPupils()
    {
        await WriteFileAsync(CreateRow(1, "P1"));

        await Assert.ThrowsAsync<ArgumentException>(() => WriteFileAsync());

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(1, await dbContext.PupilPremiumPupils.CountAsync());
    }

    private Task<PupilPremiumFileWriteResult> WriteFileAsync(params PupilPremiumFileRow[] rows) =>
        _writer.WriteFileAsync(AcademicYear, IngestVersion.Unamended, ToAsync(rows));

    private async Task AddRunAsync(
        string academicYear,
        IngestVersion ingestVersion,
        IngestRunStatus status,
        IngestDataset dataset = IngestDataset.PupilPremium)
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        string path = $"pupil-premium/{Guid.NewGuid()}.zip";
        DateTime now = DateTime.UtcNow;

        dbContext.IngestFiles.Add(new IngestFile
        {
            Path = path,
            BlobName = path,
            Status = IngestFileStatus.Succeeded,
            FirstSeenAt = now
        });
        dbContext.IngestRuns.Add(new IngestRun
        {
            Id = Guid.CreateVersion7(),
            FilePath = path,
            Attempt = 1,
            DataFileName = "NPD_pupil-premium.txt",
            Dataset = dataset,
            AcademicYear = academicYear,
            IngestVersion = ingestVersion,
            Status = status,
            StartedAt = now,
            FinishedAt = now
        });
        await dbContext.SaveChangesAsync();
    }

    private static async IAsyncEnumerable<PupilPremiumFileRow> ToAsync(params PupilPremiumFileRow[] rows)
    {
        foreach (PupilPremiumFileRow row in rows)
        {
            await Task.Yield();
            yield return row;
        }
    }

    private static PupilPremiumFileRow CreateRow(
        int rowNumber,
        string pupilMatchingRef,
        string surname = "Smith",
        string? source = "C",
        string data = "{}") =>
        new(
            rowNumber,
            pupilMatchingRef,
            Upn: "A123456789012",
            LaCode: 201,
            EstablishmentNumber: 3614,
            Urn: 100000,
            Surname: surname,
            Forename: "Alex",
            MiddleNames: "Jo",
            Sex: "M",
            Dob: new DateOnly(2015, 2, 1),
            Source: source,
            Data: data);
}
