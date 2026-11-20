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
public sealed class PupilPremiumIngestTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string Header = "PupilMatchingRef\tLA\tEstab\tURN\tUPN\tSurname\tForename\tMiddlenames\tSex\tDOB\tSource\tAdopted";

    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobContainerClient _metadataContainerClient;
    private readonly IngestFileProcessor _processor;

    public PupilPremiumIngestTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, IngestStorage.ContainerName);
        _metadataContainerClient = new BlobContainerClient(azurite.ConnectionString, IngestMetadataPublisher.ContainerName);

        GiapPostgresDbContextFactory dbContextFactory = new(postgres);

        _processor = new IngestFileProcessor(
            new IngestStorage(_containerClient, TimeProvider.System),
            new IngestDatasetProcessorRegistry([new PupilPremiumDatasetProcessor(new PupilPremiumDataWriter(dbContextFactory))]),
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
        await dbContext.PupilPremiumPupils.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ProcessAsync_PupilPremiumZip_WritesPupilsAndRejectedRowsAndPublishesMetadata()
    {
        await UploadAndProcessAsync(
            "NPD_pupil-premium_2425.zip",
            ("NPD_pupil-premium_FFD_CLA_2425_U.txt",
                Header + "\tNCYear\n" +
                "P1\t201\t3614\t100000\tA123456789012\tSmith\tAlex\tJo\tM\t01/02/2015\tC\t0\t5\n" +
                "P2\t201\t3614\t100000\tB123456789012\tJones\tSam\t\tF\t2015-03-01\tC\t1\t5\n" +
                "P1\t202\t\t\tA123456789012\tSmith\tAlex\tJo\tM\t01/02/2015\tA\t\t6\n"),
            ("Pupil premium metadata.xlsx", "metadata"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal(IngestFileStatus.Succeeded, (await dbContext.IngestFiles.SingleAsync()).Status);

        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(
            (IngestDataset.PupilPremium, "2024/2025", IngestVersion.Unamended),
            (run.Dataset, run.AcademicYear, run.IngestVersion));
        Assert.Equal((3, 1, 0, 1), (run.RecordsRead, run.RecordsWritten, run.RecordsRemoved, run.RecordsRejected));

        IngestRejectedRow rejected = await dbContext.IngestRejectedRows.SingleAsync();
        Assert.Equal(2, rejected.RowNumber);
        Assert.Equal(["Adopted is 1; pupils adopted from care aren't loaded."], rejected.Reasons);

        PupilPremiumPupil pupil = await dbContext.PupilPremiumPupils.SingleAsync();
        Assert.Equal(
            ("P1", 201, "Jo", new DateOnly(2015, 2, 1), "Both"),
            (pupil.PupilMatchingRef, pupil.LaCode, pupil.MiddleNames, pupil.Dob, pupil.SourceFlag));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(
                """
                [
                    {"PupilMatchingRef": "P1", "LA": "201", "Estab": "3614", "URN": "100000", "UPN": "A123456789012", "Surname": "Smith", "Forename": "Alex", "Middlenames": "Jo", "Sex": "M", "DOB": "2015-02-01T00:00:00.000Z", "Source": "C", "AdoptedfromCarePremium": "0", "NCYear": "5"},
                    {"PupilMatchingRef": "P1", "LA": "202", "Estab": null, "URN": null, "UPN": "A123456789012", "Surname": "Smith", "Forename": "Alex", "Middlenames": "Jo", "Sex": "M", "DOB": "2015-02-01T00:00:00.000Z", "Source": "A", "AdoptedfromCarePremium": null, "NCYear": "6"}
                ]
                """),
            JsonNode.Parse(pupil.Data)));

        Assert.True(await _metadataContainerClient.GetBlobClient("Pupil premium metadata.xlsx").ExistsAsync());
    }

    [Fact]
    public async Task ProcessAsync_AmendedFile_FailsDataFile()
    {
        await UploadAndProcessAsync("NPD_pupil-premium_2425.zip", ("NPD_pupil-premium_2425_A.txt", Header + "\n" + Row("P1")));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("A pupil premium file must be U or F, not Amended.", run.Error);
        Assert.Empty(await dbContext.PupilPremiumPupils.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_FileOlderThanStoredData_FailsDataFile()
    {
        await UploadAndProcessAsync("NPD_pupil-premium_2425_F.zip", ("NPD_pupil-premium_2425_F.txt", Header + "\n" + Row("P1")));
        await UploadAndProcessAsync("NPD_pupil-premium_2425_U.zip", ("NPD_pupil-premium_2425_U.txt", Header + "\n" + Row("P2")));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync(r => r.DataFileName == "NPD_pupil-premium_2425_U.txt");
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal(
            "The pupil premium file is 2024/2025 Unamended but the stored data is from a more recent 2024/2025 Final file.",
            run.Error);
        Assert.Equal("P1", (await dbContext.PupilPremiumPupils.SingleAsync()).PupilMatchingRef);
    }

    [Fact]
    public async Task ProcessAsync_Replay_LeavesTheSameData()
    {
        await UploadAndProcessAsync("NPD_pupil-premium_2425.zip", ("NPD_pupil-premium_2425_F.txt", Header + "\n" + Row("P1")));
        PupilPremiumPupil before = await GetPupilAsync();

        await _processor.ProcessAsync("pupil-premium/NPD_pupil-premium_2425.zip");

        Assert.Equivalent(before, await GetPupilAsync());

        async Task<PupilPremiumPupil> GetPupilAsync()
        {
            await using GiapDbContext dbContext = _postgres.CreateDbContext();
            return await dbContext.PupilPremiumPupils.SingleAsync();
        }
    }

    private static string Row(string pupilMatchingRef) =>
        $"{pupilMatchingRef}\t201\t3614\t100000\tA123456789012\tSmith\tAlex\t\tM\t2015-02-01\tC\t0";

    private async Task UploadAndProcessAsync(string zipName, params (string Name, string Content)[] entries)
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

        string blobName = $"pupil-premium/upload/{zipName}";
        string path = $"pupil-premium/{zipName}";

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
