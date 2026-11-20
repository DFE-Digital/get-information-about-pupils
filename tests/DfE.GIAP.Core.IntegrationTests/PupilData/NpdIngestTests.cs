using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.PupilData;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DfE.GIAP.Core.IntegrationTests.PupilData;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class NpdIngestTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string FilePath = "census/NPD_Census_2425.zip";

    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobContainerClient _metadataContainerClient;
    private readonly IngestFileProcessor _processor;

    public NpdIngestTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, IngestStorage.ContainerName);
        _metadataContainerClient = new BlobContainerClient(azurite.ConnectionString, IngestMetadataPublisher.ContainerName);

        GiapPostgresDbContextFactory dbContextFactory = new(postgres);

        _processor = new IngestFileProcessor(
            new IngestStorage(_containerClient, TimeProvider.System),
            new IngestDatasetProcessorRegistry([new NpdDatasetProcessor(new NpdDataWriter(dbContextFactory))]),
            new IngestMetadataPublisher(_metadataContainerClient),
            dbContextFactory,
            TimeProvider.System,
            NullLogger<IngestFileProcessor>.Instance);
    }

    public async Task InitializeAsync()
    {
        await _containerClient.DeleteIfExistsAsync();
        await _containerClient.CreateAsync();
        await _metadataContainerClient.DeleteIfExistsAsync();
        await _metadataContainerClient.CreateAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        await dbContext.IngestRejectedRows.ExecuteDeleteAsync();
        await dbContext.IngestRuns.ExecuteDeleteAsync();
        await dbContext.IngestFiles.ExecuteDeleteAsync();
        await dbContext.NpdPupilRecords.ExecuteDeleteAsync();
        await dbContext.NpdPupils.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ProcessAsync_CensusZip_WritesRecordsAndRejectedRowsAndPublishesMetadata()
    {
        await UploadAndProcessAsync(
            ("NPD_Census_2425_KtS_Autumn.txt",
                """
                PupilMatchingRef	AcademicYear	LA	Estab	URN	UPN	Surname	Forename	MiddleNames	Sex	DOB	EntryDate	Pupil.Type
                P1	2024/2025	201	3614	100000	A123456789012	Smith	Alex		M	01/02/2015	2019-09-01	NULL
                P2	2024/2025	201	3614	100000	B123456789012	Jones	Sam		F	31/02/2015	2019-09-01	X
                P1	2024/2025	202	1234	100001	A123456789012	Smith	Alex		M	01/02/2015	2020-09-01	Y
                """),
            ("Census metadata.xlsx", "metadata"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal(IngestFileStatus.Succeeded, (await dbContext.IngestFiles.SingleAsync()).Status);

        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal((3, 1, 0, 1), (run.RecordsRead, run.RecordsWritten, run.RecordsRemoved, run.RecordsRejected));

        IngestRejectedRow rejected = await dbContext.IngestRejectedRows.SingleAsync();
        Assert.Equal(2, rejected.RowNumber);
        Assert.Equal(["DOB isn't a valid date: '31/02/2015'."], rejected.Reasons);

        NpdPupilRecord record = await dbContext.NpdPupilRecords.SingleAsync();
        Assert.Equal(
            (NpdDataset.CensusAutumn, "2024/2025", IngestVersion.Unamended, 201, new DateOnly(2015, 2, 1)),
            (record.Dataset, record.AcademicYear, record.IngestVersion, record.LaCode, record.Dob));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                """
                [
                    {"PupilMatchingRef": "P1", "AcademicYear": "2024/2025", "LA": "201", "Estab": "3614", "URN": "100000", "UPN": "A123456789012", "Surname": "Smith", "Forename": "Alex", "MiddleNames": null, "Sex": "M", "DOB": "2015-02-01T00:00:00.000Z", "EntryDate": "2019-09-01T00:00:00.000Z", "Pupil_Type": "NULL"},
                    {"PupilMatchingRef": "P1", "AcademicYear": "2024/2025", "LA": "202", "Estab": "1234", "URN": "100001", "UPN": "A123456789012", "Surname": "Smith", "Forename": "Alex", "MiddleNames": null, "Sex": "M", "DOB": "2015-02-01T00:00:00.000Z", "EntryDate": "2020-09-01T00:00:00.000Z", "Pupil_Type": "Y"}
                ]
                """),
            JsonNode.Parse(record.Data)));
        Assert.Equal("P1", (await dbContext.NpdPupils.SingleAsync()).PupilMatchingRef);

        Assert.True(await _metadataContainerClient.GetBlobClient("Census metadata.xlsx").ExistsAsync());
    }

    [Fact]
    public async Task ProcessAsync_Replay_LeavesTheSameData()
    {
        await UploadAndProcessAsync(
            ("NPD_Census_2425_KtS_Spring.txt",
                """
                PupilMatchingRef	AcademicYear	LA	Estab	URN	UPN	Surname	Forename	MiddleNames	Sex	DOB
                P1	2024/2025	201	3614	100000	A123456789012	Smith	Alex		M	2015-02-01
                """));
        NpdPupilRecord before = await GetRecordAsync();

        await _processor.ProcessAsync(FilePath);

        Assert.Equivalent(before, await GetRecordAsync());

        async Task<NpdPupilRecord> GetRecordAsync()
        {
            await using GiapDbContext dbContext = _postgres.CreateDbContext();
            return await dbContext.NpdPupilRecords.SingleAsync();
        }
    }

    [Fact]
    public async Task ProcessAsync_EveryRowRejected_FailsDataFile()
    {
        await UploadAndProcessAsync(
            ("NPD_Census_2425_KtS_Autumn.txt",
                """
                PupilMatchingRef	AcademicYear	LA	Estab	URN	UPN	Surname	Forename	MiddleNames	Sex	DOB
                	2024/2025	201	3614	100000	A123456789012	Smith	Alex		M	2015-02-01
                P2	2023/2024	201	3614	100000	A123456789012	Smith	Alex		M	2015-02-01
                """));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("All 2 rows in the data file were rejected. Row 1: PupilMatchingRef is missing.", run.Error);
        Assert.Empty(await dbContext.NpdPupilRecords.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_LessMatureFile_FailsDataFile()
    {
        await UploadAndProcessAsync(
            ("NPD_Census_2425_KtS_Summer_F.txt",
                """
                PupilMatchingRef	AcademicYear	LA	Estab	URN	UPN	Surname	Forename	MiddleNames	Sex	DOB
                P1	2024/2025	201	3614	100000	A123456789012	Smith	Alex		M	2015-02-01
                """));

        await _containerClient.DeleteIfExistsAsync();
        await _containerClient.CreateAsync();
        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            await dbContext.IngestRuns.ExecuteDeleteAsync();
            await dbContext.IngestFiles.ExecuteDeleteAsync();
        }

        await UploadAndProcessAsync(
            ("NPD_Census_2425_KtS_Summer_U.txt",
                """
                PupilMatchingRef	AcademicYear	LA	Estab	URN	UPN	Surname	Forename	MiddleNames	Sex	DOB
                P2	2024/2025	201	3614	100000	A123456789012	Smith	Alex		M	2015-02-01
                """));

        await using GiapDbContext context = _postgres.CreateDbContext();
        IngestRun run = await context.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("The CensusSummer 2024/2025 file is Unamended but the stored data is from a more mature Final file.", run.Error);
        Assert.Equal("P1", (await context.NpdPupilRecords.SingleAsync()).PupilMatchingRef);
    }

    private async Task UploadAndProcessAsync(params (string Name, string Content)[] entries)
    {
        using MemoryStream stream = new();

        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach ((string name, string content) in entries)
            {
                await using Stream entryStream = archive.CreateEntry(name).Open();
                await entryStream.WriteAsync(Encoding.UTF8.GetBytes(content));
            }
        }

        stream.Position = 0;
        await _containerClient.UploadBlobAsync("census/upload/NPD_Census_2425.zip", stream);

        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            dbContext.IngestFiles.Add(new IngestFile
            {
                Path = FilePath,
                BlobName = "census/upload/NPD_Census_2425.zip",
                Status = IngestFileStatus.Pending,
                FirstSeenAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        try
        {
            await _processor.ProcessAsync(FilePath);
        }
        catch (IngestFileFailedException)
        {
        }
    }
}
