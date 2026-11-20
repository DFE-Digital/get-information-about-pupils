using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.PupilData;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Core.IntegrationTests.PupilData;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class NpdDataWriterTests(GiapPostgresFixture postgres) : IAsyncLifetime
{
    private const string AcademicYear = "2023/2024";

    private readonly NpdDataWriter _writer = new(new GiapPostgresDbContextFactory(postgres));

    public async Task InitializeAsync()
    {
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        await dbContext.NpdPupilRecords.ExecuteDeleteAsync();
        await dbContext.NpdPupils.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task WriteFileAsync_WritesRecordsAndPupils()
    {
        NpdFileRow row = CreateRow(1, "P1", data: """{"KS2_MAT": "110"}""");

        NpdFileWriteResult result = await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, row);

        Assert.Equal(new NpdFileWriteResult(RecordsWritten: 1, RecordsRemoved: 0), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        // CreatedAt is set by the database
        Assert.Equivalent(
            new
            {
                row.PupilMatchingRef,
                Dataset = NpdDataset.Ks2,
                AcademicYear,
                IngestVersion = IngestVersion.Final,
                row.Upn,
                row.LaCode,
                row.EstablishmentNumber,
                row.Urn,
                row.Surname,
                row.Forename,
                row.MiddleNames,
                row.Sex,
                row.Dob,
                Data = """[{"KS2_MAT": "110"}]"""
            },
            await dbContext.NpdPupilRecords.SingleAsync());
        Assert.Equivalent(
            new NpdPupil
            {
                PupilMatchingRef = "P1",
                Upn = "A123456789012",
                LaCode = 201,
                EstablishmentNumber = 3614,
                Urn = 100000,
                Surname = "Smith",
                Forename = "Alex",
                MiddleNames = null,
                Sex = "M",
                Dob = new DateOnly(2013, 9, 1)
            },
            await dbContext.NpdPupils.SingleAsync());
    }

    [Fact]
    public async Task WriteFileAsync_PupilWithMoreThanOneRow_CombinesRowsInFileOrderWithFieldsFromFirstRow()
    {
        NpdFileWriteResult result = await WriteFileAsync(
            NpdDataset.CensusAutumn,
            IngestVersion.Final,
            CreateRow(1, "P1", surname: "First", data: """{"URN": "1"}"""),
            CreateRow(2, "P2", data: """{"URN": "2"}"""),
            CreateRow(3, "P1", surname: "Second", data: """{"URN": "3"}"""));

        Assert.Equal(new NpdFileWriteResult(RecordsWritten: 2, RecordsRemoved: 0), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        NpdPupilRecord record = await dbContext.NpdPupilRecords.SingleAsync(r => r.PupilMatchingRef == "P1");
        Assert.Equal("First", record.Surname);
        Assert.Equal("""[{"URN": "1"}, {"URN": "3"}]""", record.Data);
    }

    [Theory]
    [InlineData(IngestVersion.Unamended, IngestVersion.Amended)]
    [InlineData(IngestVersion.Amended, IngestVersion.Final)]
    [InlineData(IngestVersion.Amended, IngestVersion.Amended)]
    public async Task WriteFileAsync_EquallyOrMoreMatureFile_ReplacesStoredRecords(IngestVersion storedVersion, IngestVersion incomingVersion)
    {
        await WriteFileAsync(NpdDataset.Ks2, storedVersion, CreateRow(1, "P1", data: """{"KS2_MAT": "100"}"""));

        await WriteFileAsync(NpdDataset.Ks2, incomingVersion, CreateRow(1, "P1", data: """{"KS2_MAT": "110"}"""));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        NpdPupilRecord stored = await dbContext.NpdPupilRecords.SingleAsync();
        Assert.Equal(incomingVersion, stored.IngestVersion);
        Assert.Equal("""[{"KS2_MAT": "110"}]""", stored.Data);
    }

    [Theory]
    [InlineData(IngestVersion.Amended, IngestVersion.Unamended)]
    [InlineData(IngestVersion.Final, IngestVersion.Amended)]
    public async Task WriteFileAsync_LessMatureFile_ThrowsAndLeavesStoredRecords(IngestVersion storedVersion, IngestVersion incomingVersion)
    {
        await WriteFileAsync(NpdDataset.Ks2, storedVersion, CreateRow(1, "P1"));

        NpdFileVersionException exception = await Assert.ThrowsAsync<NpdFileVersionException>(
            () => WriteFileAsync(NpdDataset.Ks2, incomingVersion, CreateRow(1, "P1"), CreateRow(2, "P2")));

        Assert.Equal(storedVersion, exception.StoredIngestVersion);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(storedVersion, (await dbContext.NpdPupilRecords.SingleAsync()).IngestVersion);
        Assert.Equal(["P1"], await dbContext.NpdPupils.Select(p => p.PupilMatchingRef).ToArrayAsync());
    }

    [Fact]
    public async Task WriteFileAsync_LessMatureFileForAnotherYear_IsWritten()
    {
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRow(1, "P1"));

        await _writer.WriteFileAsync(NpdDataset.Ks2, "2024/2025", IngestVersion.Unamended, ToAsync(CreateRow(1, "P1")));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(2, await dbContext.NpdPupilRecords.CountAsync());
    }

    [Fact]
    public async Task WriteFileAsync_PupilMissingFromNewFile_LosesRecordForThatDatasetAndYearOnly()
    {
        await WriteFileAsync(NpdDataset.CensusSpring, IngestVersion.Final, CreateRow(1, "P1"));
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Unamended, CreateRow(1, "P1"), CreateRow(2, "P2"));

        NpdFileWriteResult result = await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Amended, CreateRow(1, "P3"));

        Assert.Equal(new NpdFileWriteResult(RecordsWritten: 1, RecordsRemoved: 2), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(
            [("P1", NpdDataset.CensusSpring), ("P3", NpdDataset.Ks2)],
            (await dbContext.NpdPupilRecords.OrderBy(r => r.PupilMatchingRef).ToArrayAsync()).Select(r => (r.PupilMatchingRef, r.Dataset)));
        // P2 has no records left
        Assert.Equal(["P1", "P3"], await dbContext.NpdPupils.OrderBy(p => p.PupilMatchingRef).Select(p => p.PupilMatchingRef).ToArrayAsync());
    }

    [Fact]
    public async Task WriteFileAsync_PupilFields_ComeFromCensusRecordBeforeLaterRecords()
    {
        await WriteFileAsync(NpdDataset.CensusSpring, IngestVersion.Final, CreateRow(1, "P1", surname: "Census"));
        await _writer.WriteFileAsync(NpdDataset.Ks4, "2024/2025", IngestVersion.Final, ToAsync(CreateRow(1, "P1", surname: "Ks4")));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("Census", (await dbContext.NpdPupils.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_PupilFields_ComeFromLatestAcademicYear()
    {
        await _writer.WriteFileAsync(NpdDataset.Ks4, "2024/2025", IngestVersion.Final, ToAsync(CreateRow(1, "P1", surname: "Later")));
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRow(1, "P1", surname: "Earlier"));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("Later", (await dbContext.NpdPupils.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_PupilFields_ComeFromFirstLoadedRecordWhenOtherwiseTied()
    {
        await WriteFileAsync(NpdDataset.CensusSummer, IngestVersion.Final, CreateRow(1, "P1", surname: "First"));
        await WriteFileAsync(NpdDataset.CensusAutumn, IngestVersion.Final, CreateRow(1, "P1", surname: "Second"));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("First", (await dbContext.NpdPupils.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_ReprocessedFile_KeepsCreatedAt()
    {
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Amended, CreateRow(1, "P1"));
        DateTime createdAt = await GetCreatedAtAsync();

        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Amended, CreateRow(1, "P1"));
        Assert.Equal(createdAt, await GetCreatedAtAsync());

        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRow(1, "P1"));
        Assert.True(await GetCreatedAtAsync() > createdAt);

        async Task<DateTime> GetCreatedAtAsync()
        {
            await using GiapDbContext dbContext = postgres.CreateDbContext();
            return (await dbContext.NpdPupilRecords.SingleAsync()).CreatedAt;
        }
    }

    [Fact]
    public async Task WriteFileAsync_NoRows_ThrowsAndLeavesStoredRecords()
    {
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRow(1, "P1"));

        await Assert.ThrowsAsync<ArgumentException>(() => WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(1, await dbContext.NpdPupilRecords.CountAsync());
    }

    private Task<NpdFileWriteResult> WriteFileAsync(NpdDataset dataset, IngestVersion ingestVersion, params NpdFileRow[] rows) =>
        _writer.WriteFileAsync(dataset, AcademicYear, ingestVersion, ToAsync(rows));

    private static async IAsyncEnumerable<NpdFileRow> ToAsync(params NpdFileRow[] rows)
    {
        foreach (NpdFileRow row in rows)
        {
            await Task.Yield();
            yield return row;
        }
    }

    private static NpdFileRow CreateRow(int rowNumber, string pupilMatchingRef, string surname = "Smith", string data = "{}") =>
        new(
            rowNumber,
            pupilMatchingRef,
            Upn: "A123456789012",
            LaCode: 201,
            EstablishmentNumber: 3614,
            Urn: 100000,
            Surname: surname,
            Forename: "Alex",
            MiddleNames: null,
            Sex: "M",
            Dob: new DateOnly(2013, 9, 1),
            Data: data);
}
