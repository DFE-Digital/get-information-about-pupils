using System.IO.Compression;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
public sealed class ChildrenLookedAfterIngestTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string Header = "CLA_LA\tCLA_CHILD_LA_CODE\tCLA_SEX\tCLA_DOB\tCLA_AGE_AUG\tCLA_UPN\tCLA_ETHNIC\tCLA_12_MONTHS";

    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobContainerClient _metadataContainerClient;
    private readonly BlobContainerClient _downloadsContainerClient;
    private readonly IngestFileProcessor _processor;

    public ChildrenLookedAfterIngestTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, IngestStorage.ContainerName);
        _metadataContainerClient = new BlobContainerClient(azurite.ConnectionString, IngestMetadataPublisher.ContainerName);
        _downloadsContainerClient = new BlobContainerClient(azurite.ConnectionString, PreparedDownloadPublisher.ContainerName);

        GiapPostgresDbContextFactory dbContextFactory = new(postgres);

        ChildrenLookedAfterDatasetProcessor datasetProcessor = new(new PreparedDownloadWriter(
            dbContextFactory,
            new PreparedDownloadPublisher(_downloadsContainerClient),
            NullLogger<PreparedDownloadWriter>.Instance));

        _processor = new IngestFileProcessor(
            new IngestStorage(_containerClient, TimeProvider.System),
            new IngestDatasetProcessorRegistry([datasetProcessor]),
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
        await _downloadsContainerClient.DeleteIfExistsAsync();
        await _downloadsContainerClient.CreateAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        await dbContext.IngestRejectedRows.ExecuteDeleteAsync();
        await dbContext.IngestRuns.ExecuteDeleteAsync();
        await dbContext.IngestFiles.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ProcessAsync_Ks4Zip_WritesAFilePerLaAndPublishesMetadata()
    {
        await UploadAndProcessAsync(
            "cla-ks4",
            "GIAP_CLA_KS4_2025.zip",
            ("GIAP_CLA_KS4_2025.txt",
                Header + "\tExtra\n" +
                "202\t202\tM\t01/02/2009\t15\tA123456789012\tWBRI\t1\tx\n" +
                "201\t201\tF\t2009-03-01\t15\tB123456789012\tWBRI\t0\t\n" +
                "\t203\tF\t2009-03-01\t15\tC123456789012\tWBRI\t0\t\n" +
                "201\t201\tM\t2009-04-01\t15\tD123456789012\tAOTH\t1\ty, z\n"),
            ("CLA metadata.docx", "metadata"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal(IngestFileStatus.Succeeded, (await dbContext.IngestFiles.SingleAsync()).Status);

        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(
            (IngestDataset.ChildrenLookedAfterKs4, "2024/2025", IngestVersion.Unamended),
            (run.Dataset, run.AcademicYear, run.IngestVersion));
        Assert.Equal((4, 3, 0, 0), (run.RecordsRead, run.RecordsWritten, run.RecordsRemoved, run.RecordsRejected));

        Assert.Equal(["LA/201/ks4_2025_cla.csv", "LA/202/ks4_2025_cla.csv"], await ListDownloadsAsync());
        Assert.Equal(
            "CLA_LA,CLA_CHILD_LA_CODE,CLA_SEX,CLA_DOB,CLA_AGE_AUG,CLA_UPN,CLA_ETHNIC,CLA_12_MONTHS,Extra\n" +
            "201,201,F,2009-03-01,15,B123456789012,WBRI,0,\n" +
            "201,201,M,2009-04-01,15,D123456789012,AOTH,1,\"y, z\"\n",
            await DownloadAsync("LA/201/ks4_2025_cla.csv"));
        Assert.Equal(
            "CLA_LA,CLA_CHILD_LA_CODE,CLA_SEX,CLA_DOB,CLA_AGE_AUG,CLA_UPN,CLA_ETHNIC,CLA_12_MONTHS,Extra\n" +
            "202,202,M,01/02/2009,15,A123456789012,WBRI,1,x\n",
            await DownloadAsync("LA/202/ks4_2025_cla.csv"));

        Assert.True(await _metadataContainerClient.GetBlobClient("CLA metadata.docx").ExistsAsync());
    }

    [Fact]
    public async Task ProcessAsync_Ks2Zip_WritesKs2Files()
    {
        await UploadAndProcessAsync(
            "cla-ks2",
            "GIAP_CLA_KS2_2024.zip",
            ("GIAP_CLA_KS2_2024.txt", Header + "\n201\t201\tF\t2013-03-01\t11\tB123456789012\tWBRI\t0\n"));

        Assert.Equal(["LA/201/ks2_2024_cla.csv"], await ListDownloadsAsync());
    }

    [Fact]
    public async Task ProcessAsync_FileMissingAColumn_FailsDataFile()
    {
        await UploadAndProcessAsync(
            "cla-ks4",
            "GIAP_CLA_KS4_2025.zip",
            ("GIAP_CLA_KS4_2025.txt", "CLA_LA\tCLA_UPN\n201\tB123456789012\n"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.StartsWith("The data file is missing the following columns: 'CLA_CHILD_LA_CODE'", run.Error);
        Assert.Empty(await ListDownloadsAsync());
    }

    [Fact]
    public async Task ProcessAsync_Ks2FileInKs4Folder_FailsDataFile()
    {
        await UploadAndProcessAsync(
            "cla-ks4",
            "GIAP_CLA_KS4_2025.zip",
            ("GIAP_CLA_KS2_2025.txt", Header + "\n201\t201\tF\t2013-03-01\t11\tB123456789012\tWBRI\t0\n"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("ChildrenLookedAfterKs2 data files can't be uploaded to cla-ks4.", run.Error);
    }

    private async Task<List<string>> ListDownloadsAsync()
    {
        List<string> names = [];

        await foreach (BlobItem blob in _downloadsContainerClient.GetBlobsAsync())
        {
            names.Add(blob.Name);
        }

        return names;
    }

    private async Task<string> DownloadAsync(string blobName) =>
        (await _downloadsContainerClient.GetBlobClient(blobName).DownloadContentAsync()).Value.Content.ToString();

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
