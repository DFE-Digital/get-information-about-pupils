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
public sealed class FurtherEducationIngestTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string Header = "ULN\tSurname\tForename\tSex\tDOB\tACAD_YEAR";

    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobContainerClient _metadataContainerClient;
    private readonly IngestFileProcessor _processor;

    public FurtherEducationIngestTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, IngestStorage.ContainerName);
        _metadataContainerClient = new BlobContainerClient(azurite.ConnectionString, IngestMetadataPublisher.ContainerName);

        GiapPostgresDbContextFactory dbContextFactory = new(postgres);

        _processor = new IngestFileProcessor(
            new IngestStorage(_containerClient, TimeProvider.System),
            new IngestDatasetProcessorRegistry([new FurtherEducationDatasetProcessor(new FurtherEducationDataWriter(dbContextFactory))]),
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
        await dbContext.FurtherEducationLearnerRecords.ExecuteDeleteAsync();
        await dbContext.FurtherEducationLearners.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ProcessAsync_PupilPremiumZip_WritesRecordsAndRejectedRowsAndPublishesMetadata()
    {
        await UploadAndProcessAsync(
            "fe_pp",
            "FE_PP_2025.zip",
            ("FE_PP_2025.txt",
                Header + "\tFTE\n" +
                "1\tSmith\tAlex\tM\t01/02/2005\t2024/2025\t1\n" +
                "2\tJones\tSam\tF\t2005-03-01\t2023/2024\t1\n" +
                "1\tSmith\tAlex\tM\t01/02/2005\t\t0.5\n"),
            ("FE pupil premium metadata.xlsx", "metadata"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal(IngestFileStatus.Succeeded, (await dbContext.IngestFiles.SingleAsync()).Status);

        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(
            (IngestDataset.FurtherEducationPupilPremium, "2024/2025", IngestVersion.Unamended),
            (run.Dataset, run.AcademicYear, run.IngestVersion));
        Assert.Equal((3, 1, 0, 1), (run.RecordsRead, run.RecordsWritten, run.RecordsRemoved, run.RecordsRejected));

        IngestRejectedRow rejected = await dbContext.IngestRejectedRows.SingleAsync();
        Assert.Equal(2, rejected.RowNumber);
        Assert.Equal(["ACAD_YEAR '2023/2024' doesn't match the file's academic year, 2024/2025."], rejected.Reasons);

        FurtherEducationLearnerRecord record = await dbContext.FurtherEducationLearnerRecords.SingleAsync();
        Assert.Equal(
            ("1", FurtherEducationDataset.PupilPremium, "2024/2025", new DateOnly(2005, 2, 1)),
            (record.Uln, record.Dataset, record.AcademicYear, record.Dob));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                """
                [
                    {"ULN": "1", "Surname": "Smith", "Forename": "Alex", "Sex": "M", "DOB": "2005-02-01T00:00:00.000Z", "ACAD_YEAR": "2024/2025", "FTE": "1"},
                    {"ULN": "1", "Surname": "Smith", "Forename": "Alex", "Sex": "M", "DOB": "2005-02-01T00:00:00.000Z", "ACAD_YEAR": null, "FTE": "0.5"}
                ]
                """),
            JsonNode.Parse(record.Data)));
        Assert.Equal("1", (await dbContext.FurtherEducationLearners.SingleAsync()).Uln);

        Assert.True(await _metadataContainerClient.GetBlobClient("FE pupil premium metadata.xlsx").ExistsAsync());
    }

    [Fact]
    public async Task ProcessAsync_SenZip_WritesRecords()
    {
        await UploadAndProcessAsync("fe_sen", "FE_SEN_2025.zip", ("FE_SEN_2025.txt", Header + "\n" + Row("1")));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        FurtherEducationLearnerRecord record = await dbContext.FurtherEducationLearnerRecords.SingleAsync();
        Assert.Equal(
            ("1", FurtherEducationDataset.SpecialEducationalNeeds, "2024/2025"),
            (record.Uln, record.Dataset, record.AcademicYear));
    }

    [Fact]
    public async Task ProcessAsync_LessMatureFile_IsWritten()
    {
        await UploadAndProcessAsync("fe_sen", "FE_SEN_2025_F.zip", ("FE_SEN_2025_F.txt", Header + "\n" + Row("1")));
        await UploadAndProcessAsync("fe_sen", "FE_SEN_2025_U.zip", ("FE_SEN_2025_U.txt", Header + "\n" + Row("2")));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.All(await dbContext.IngestRuns.ToListAsync(), r => Assert.Equal(IngestRunStatus.Succeeded, r.Status));
        Assert.Equal("2", (await dbContext.FurtherEducationLearnerRecords.SingleAsync()).Uln);
    }

    [Fact]
    public async Task ProcessAsync_EveryRowRejected_FailsDataFile()
    {
        await UploadAndProcessAsync("fe_pp", "FE_PP_2025.zip", ("FE_PP_2025.txt", Header + "\n" + Row("")));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("All 1 rows in the data file were rejected. Row 1: ULN is missing.", run.Error);
        Assert.Empty(await dbContext.FurtherEducationLearnerRecords.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_Replay_LeavesTheSameData()
    {
        await UploadAndProcessAsync("fe_pp", "FE_PP_2025.zip", ("FE_PP_2025.txt", Header + "\n" + Row("1")));
        FurtherEducationLearnerRecord before = await GetRecordAsync();

        await _processor.ProcessAsync("fe_pp/FE_PP_2025.zip");

        Assert.Equivalent(before, await GetRecordAsync());

        async Task<FurtherEducationLearnerRecord> GetRecordAsync()
        {
            await using GiapDbContext dbContext = _postgres.CreateDbContext();
            return await dbContext.FurtherEducationLearnerRecords.SingleAsync();
        }
    }

    private static string Row(string uln) => $"{uln}\tSmith\tAlex\tM\t2005-02-01\t2024/2025";

    private async Task UploadAndProcessAsync(string folder, string zipName, params (string Name, string Content)[] entries)
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

        string blobName = $"{folder}/upload/{zipName}";
        string path = $"{folder}/{zipName}";

        stream.Position = 0;
        await _containerClient.UploadBlobAsync(blobName, stream);

        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            dbContext.IngestFiles.Add(new IngestFile
            {
                Path = path,
                BlobName = blobName,
                Status = IngestFileStatus.Pending,
                FirstSeenAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        try
        {
            await _processor.ProcessAsync(path);
        }
        catch (IngestFileFailedException)
        {
        }
    }
}
