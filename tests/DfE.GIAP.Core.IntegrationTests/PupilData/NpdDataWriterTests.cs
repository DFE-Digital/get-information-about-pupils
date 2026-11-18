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
        NpdPupilRecord record = CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Final, data: """[{"KS2_MAT": "110"}]""");

        NpdFileWriteResult result = await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, record);

        Assert.Equal(new NpdFileWriteResult(RecordsWritten: 1, RecordsRemoved: 0), result);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        // CreatedAt is set by the database
        Assert.Equivalent(
            new
            {
                record.PupilMatchingRef,
                record.Dataset,
                record.AcademicYear,
                record.IngestVersion,
                record.Upn,
                record.LaCode,
                record.EstablishmentNumber,
                record.Urn,
                record.Surname,
                record.Forename,
                record.MiddleNames,
                record.Sex,
                record.Dob,
                record.Data
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

    [Theory]
    [InlineData(IngestVersion.Unamended, IngestVersion.Amended)]
    [InlineData(IngestVersion.Amended, IngestVersion.Final)]
    [InlineData(IngestVersion.Amended, IngestVersion.Amended)]
    public async Task WriteFileAsync_EquallyOrMoreMatureFile_ReplacesStoredRecords(IngestVersion storedVersion, IngestVersion incomingVersion)
    {
        await WriteFileAsync(NpdDataset.Ks2, storedVersion, CreateRecord("P1", NpdDataset.Ks2, storedVersion, data: """[{"KS2_MAT": "100"}]"""));

        await WriteFileAsync(NpdDataset.Ks2, incomingVersion, CreateRecord("P1", NpdDataset.Ks2, incomingVersion, data: """[{"KS2_MAT": "110"}]"""));

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
        await WriteFileAsync(NpdDataset.Ks2, storedVersion, CreateRecord("P1", NpdDataset.Ks2, storedVersion));

        NpdFileVersionException exception = await Assert.ThrowsAsync<NpdFileVersionException>(
            () => WriteFileAsync(NpdDataset.Ks2, incomingVersion, CreateRecord("P1", NpdDataset.Ks2, incomingVersion), CreateRecord("P2", NpdDataset.Ks2, incomingVersion)));

        Assert.Equal(storedVersion, exception.StoredIngestVersion);
        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(storedVersion, (await dbContext.NpdPupilRecords.SingleAsync()).IngestVersion);
        Assert.Equal(["P1"], await dbContext.NpdPupils.Select(p => p.PupilMatchingRef).ToArrayAsync());
    }

    [Fact]
    public async Task WriteFileAsync_LessMatureFileForAnotherYear_IsWritten()
    {
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Final));

        await _writer.WriteFileAsync(
            NpdDataset.Ks2,
            "2024/2025",
            IngestVersion.Unamended,
            [CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Unamended, academicYear: "2024/2025")]);

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal(2, await dbContext.NpdPupilRecords.CountAsync());
    }

    [Fact]
    public async Task WriteFileAsync_PupilMissingFromNewFile_LosesRecordForThatDatasetAndYearOnly()
    {
        await WriteFileAsync(NpdDataset.CensusSpring, IngestVersion.Final, CreateRecord("P1", NpdDataset.CensusSpring, IngestVersion.Final));
        await WriteFileAsync(
            NpdDataset.Ks2,
            IngestVersion.Unamended,
            CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Unamended),
            CreateRecord("P2", NpdDataset.Ks2, IngestVersion.Unamended));

        NpdFileWriteResult result = await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Amended, CreateRecord("P3", NpdDataset.Ks2, IngestVersion.Amended));

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
        await WriteFileAsync(NpdDataset.CensusSpring, IngestVersion.Final, CreateRecord("P1", NpdDataset.CensusSpring, IngestVersion.Final, surname: "Census"));
        await _writer.WriteFileAsync(
            NpdDataset.Ks4,
            "2024/2025",
            IngestVersion.Final,
            [CreateRecord("P1", NpdDataset.Ks4, IngestVersion.Final, academicYear: "2024/2025", surname: "Ks4")]);

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("Census", (await dbContext.NpdPupils.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_PupilFields_ComeFromLatestAcademicYear()
    {
        await _writer.WriteFileAsync(
            NpdDataset.Ks4,
            "2024/2025",
            IngestVersion.Final,
            [CreateRecord("P1", NpdDataset.Ks4, IngestVersion.Final, academicYear: "2024/2025", surname: "Later")]);
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Final, surname: "Earlier"));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("Later", (await dbContext.NpdPupils.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_PupilFields_ComeFromFirstLoadedRecordWhenOtherwiseTied()
    {
        await WriteFileAsync(NpdDataset.CensusSummer, IngestVersion.Final, CreateRecord("P1", NpdDataset.CensusSummer, IngestVersion.Final, surname: "First"));
        await WriteFileAsync(NpdDataset.CensusAutumn, IngestVersion.Final, CreateRecord("P1", NpdDataset.CensusAutumn, IngestVersion.Final, surname: "Second"));

        await using GiapDbContext dbContext = postgres.CreateDbContext();
        Assert.Equal("First", (await dbContext.NpdPupils.SingleAsync()).Surname);
    }

    [Fact]
    public async Task WriteFileAsync_ReprocessedFile_KeepsCreatedAt()
    {
        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Amended, CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Amended));
        DateTime createdAt = await GetCreatedAtAsync();

        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Amended, CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Amended));
        Assert.Equal(createdAt, await GetCreatedAtAsync());

        await WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRecord("P1", NpdDataset.Ks2, IngestVersion.Final));
        Assert.True(await GetCreatedAtAsync() > createdAt);

        async Task<DateTime> GetCreatedAtAsync()
        {
            await using GiapDbContext dbContext = postgres.CreateDbContext();
            return (await dbContext.NpdPupilRecords.SingleAsync()).CreatedAt;
        }
    }

    [Fact]
    public async Task WriteFileAsync_RecordWithAnotherDataset_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => WriteFileAsync(NpdDataset.Ks2, IngestVersion.Final, CreateRecord("P1", NpdDataset.Ks4, IngestVersion.Final)));
    }

    private Task<NpdFileWriteResult> WriteFileAsync(NpdDataset dataset, IngestVersion ingestVersion, params NpdPupilRecord[] records) =>
        _writer.WriteFileAsync(dataset, AcademicYear, ingestVersion, records);

    private static NpdPupilRecord CreateRecord(
        string pupilMatchingRef,
        NpdDataset dataset,
        IngestVersion ingestVersion,
        string academicYear = AcademicYear,
        string surname = "Smith",
        string data = "[]") =>
        new()
        {
            PupilMatchingRef = pupilMatchingRef,
            Dataset = dataset,
            AcademicYear = academicYear,
            IngestVersion = ingestVersion,
            Upn = "A123456789012",
            LaCode = 201,
            EstablishmentNumber = 3614,
            Urn = 100000,
            Surname = surname,
            Forename = "Alex",
            MiddleNames = null,
            Sex = "M",
            Dob = new DateOnly(2013, 9, 1),
            Data = data
        };
}
