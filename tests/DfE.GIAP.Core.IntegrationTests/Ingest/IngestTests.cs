using System.IO.Compression;
using System.Text;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.SharedTests.Postgres;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DfE.GIAP.Core.IntegrationTests.Ingest;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class IngestTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string Ks2DataFileName = "NPD_KS2_2025_U_KtS.txt";
    private const string Ks2ZipName = "NPD_KS2_2025_U.zip";
    private const string Ks2FilePath = "ks2/" + Ks2ZipName;

    // The time of the first and second ScanAndProcessAsync, as added to the names of the zips they move
    private const string FirstTimestamp = "20261120T101500Z";
    private const string SecondTimestamp = "20261120T101600Z";

    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobContainerClient _downloadsContainerClient;
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 11, 20, 10, 15, 0, TimeSpan.Zero));
    private readonly FakeDatasetProcessor _ks2Processor = new(IngestDataset.Ks2);
    private readonly FakeDatasetProcessor _censusProcessor = new(IngestDataset.CensusAutumn, IngestDataset.CensusSpring, IngestDataset.CensusSummer);
    private readonly RecordingBackgroundJobClient _backgroundJobClient = new();
    private readonly IngestScheduler _scheduler;
    private readonly IngestFileProcessor _processor;

    public IngestTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, IngestStorage.ContainerName);
        _downloadsContainerClient = new BlobContainerClient(azurite.ConnectionString, IngestMetadataPublisher.ContainerName);

        IngestStorage storage = new(_containerClient, _timeProvider);
        GiapPostgresDbContextFactory dbContextFactory = new(postgres);

        _scheduler = new IngestScheduler(
            storage,
            dbContextFactory,
            _backgroundJobClient,
            _timeProvider,
            NullLogger<IngestScheduler>.Instance);
        _processor = new IngestFileProcessor(
            storage,
            new IngestDatasetProcessorRegistry([_ks2Processor, _censusProcessor]),
            new IngestMetadataPublisher(_downloadsContainerClient),
            dbContextFactory,
            _timeProvider,
            NullLogger<IngestFileProcessor>.Instance);
    }

    public async Task InitializeAsync()
    {
        await _containerClient.DeleteIfExistsAsync();
        await _containerClient.CreateAsync();
        await _downloadsContainerClient.DeleteIfExistsAsync();
        await _downloadsContainerClient.CreateAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        await dbContext.IngestRejectedRows.ExecuteDeleteAsync();
        await dbContext.IngestRuns.ExecuteDeleteAsync();
        await dbContext.IngestFiles.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task NewUpload_IsProcessedAndMovedToProcessed()
    {
        _ks2Processor.Result = new IngestDataFileResult(RecordsRead: 3, RecordsWritten: 2, RecordsRemoved: 1, RejectedRows: []);
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"), ("KS2 metadata.xlsx", "metadata"));

        await ScanAndProcessAsync();

        FakeDatasetProcessor.ProcessedFile processed = Assert.Single(_ks2Processor.Processed);
        Assert.Equal(Ks2DataFileName, processed.Name);
        Assert.Equal(new IngestDataFileAttributes(IngestDataset.Ks2, "2024/2025", IngestVersion.Unamended), processed.Attributes);
        Assert.Equal("data", processed.Content);

        Assert.Equal([MovedBlobName("ks2/processed/", Ks2ZipName, FirstTimestamp)], await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equivalent(
            new
            {
                Path = Ks2FilePath,
                BlobName = MovedBlobName("ks2/processed/", Ks2ZipName, FirstTimestamp),
                Status = IngestFileStatus.Succeeded,
                Error = (string?)null,
                Attempts = 1
            },
            file);

        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equivalent(
            new
            {
                FilePath = Ks2FilePath,
                Attempt = 1,
                DataFileName = Ks2DataFileName,
                Dataset = IngestDataset.Ks2,
                AcademicYear = "2024/2025",
                IngestVersion = IngestVersion.Unamended,
                RecordsRead = 3,
                RecordsWritten = 2,
                RecordsRemoved = 1,
                RecordsRejected = 0,
                Status = IngestRunStatus.Succeeded,
                Error = (string?)null
            },
            run);
        Assert.NotNull(run.FinishedAt);
    }

    [Fact]
    public async Task ProcessAsync_PublishesMetadataDocuments()
    {
        await _downloadsContainerClient.UploadBlobAsync("AllUsers/Metadata/KS2 metadata.xlsx", BinaryData.FromString("old metadata"));
        await UploadZipAsync(
            "ks2",
            Ks2ZipName,
            (Ks2DataFileName, "data"),
            ("docs/KS2 metadata.xlsx", "new metadata"),
            ("KS2 guidance.docx", "guidance"),
            ("__MACOSX/docs/._KS2 metadata.xlsx", "resource fork"),
            ("._KS2 guidance.docx", "resource fork"));

        await ScanAndProcessAsync();

        Assert.Equal(["AllUsers/Metadata/KS2 guidance.docx", "AllUsers/Metadata/KS2 metadata.xlsx"], await ListBlobsAsync(_downloadsContainerClient));
        Assert.Equal("new metadata", (await _downloadsContainerClient.GetBlobClient("AllUsers/Metadata/KS2 metadata.xlsx").DownloadContentAsync()).Value.Content.ToString());
        Assert.Equal([MovedBlobName("ks2/processed/", Ks2ZipName, FirstTimestamp)], await ListBlobsAsync());
    }

    [Fact]
    public async Task ProcessAsync_DataFileFails_DoesNotPublishMetadataDocuments()
    {
        _ks2Processor.FailOn = Ks2DataFileName;
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"), ("KS2 metadata.xlsx", "metadata"));

        await ScanAndProcessAsync();

        Assert.Empty(await ListBlobsAsync(_downloadsContainerClient));
    }

    [Fact]
    public async Task ProcessAsync_PublishingMetadataFails_FailsAndMovesToError()
    {
        await _downloadsContainerClient.DeleteAsync();
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"), ("KS2 metadata.xlsx", "metadata"));

        await ScanAndProcessAsync();

        Assert.Single(_ks2Processor.Processed);
        Assert.Equal([MovedBlobName("ks2/error/", Ks2ZipName, FirstTimestamp)], await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Failed, file.Status);
        Assert.StartsWith("Publishing the metadata document KS2 metadata.xlsx failed:", file.Error);
    }

    [Fact]
    public async Task ProcessAsync_RecordsRejectedRows()
    {
        _ks2Processor.Result = new IngestDataFileResult(
            RecordsRead: 2,
            RecordsWritten: 1,
            RecordsRemoved: 0,
            RejectedRows: [new IngestRowRejection(2, new Dictionary<string, string?> { ["UPN"] = null, ["KS2_MAT"] = "110" }, ["UPN is missing"])]);
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));

        await ScanAndProcessAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(1, run.RecordsRejected);
        IngestRejectedRow row = await dbContext.IngestRejectedRows.SingleAsync();
        Assert.Equal(run.Id, row.RunId);
        Assert.Equal(2, row.RowNumber);
        Assert.Equal("""{"UPN": null, "KS2_MAT": "110"}""", row.Data);
        Assert.Equal(["UPN is missing"], row.Reasons);
    }

    [Fact]
    public async Task ProcessAsync_ZipNameDoesNotMatchFolder_FailsAndMovesToError()
    {
        await UploadZipAsync("ks2", "NPD_KS4_2025_U.zip", ("NPD_KS4_2025_U_KtS.txt", "data"));

        await ScanAndProcessAsync();

        Assert.Empty(_ks2Processor.Processed);
        Assert.Equal([MovedBlobName("ks2/error/", "NPD_KS4_2025_U.zip", FirstTimestamp)], await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Failed, file.Status);
        Assert.Equal("The name of a zip uploaded to ks2 must start with NPD_KS2_.", file.Error);
        Assert.Empty(await dbContext.IngestRuns.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_NotAZip_Fails()
    {
        await UploadBlobAsync("ks2/upload/" + Ks2ZipName, "not a zip");

        await ScanAndProcessAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Failed, file.Status);
        Assert.Equal("The file isn't a valid zip.", file.Error);
        Assert.Equal([MovedBlobName("ks2/error/", Ks2ZipName, FirstTimestamp)], await ListBlobsAsync());
    }

    [Fact]
    public async Task ProcessAsync_ZipWithNoDataFiles_Fails()
    {
        await UploadZipAsync("ks2", Ks2ZipName, ("KS2 metadata.xlsx", "metadata"));

        await ScanAndProcessAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Failed, file.Status);
        Assert.Equal("The zip doesn't contain any data files.", file.Error);
    }

    [Fact]
    public async Task ProcessAsync_EveryDataFileIsProcessedEvenIfOneFails()
    {
        _censusProcessor.FailOn = "NPD_Census_2425_KtS_Spring.txt";
        await UploadZipAsync(
            "census",
            "NPD_Census_2425.zip",
            ("NPD_Census_2425_KtS_Autumn.txt", "autumn"),
            ("NPD_Census_2425_KtS_Spring.txt", "spring"),
            ("NPD_Census_2425_KtS_Summer.txt", "summer"));

        await ScanAndProcessAsync();

        Assert.Equal(
            ["NPD_Census_2425_KtS_Autumn.txt", "NPD_Census_2425_KtS_Spring.txt", "NPD_Census_2425_KtS_Summer.txt"],
            _censusProcessor.Processed.Select(p => p.Name));
        Assert.Equal([MovedBlobName("census/error/", "NPD_Census_2425.zip", FirstTimestamp)], await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Failed, file.Status);
        Assert.Equal("1 of the 3 data files in the zip failed.", file.Error);
        Assert.Equal(
            [
                ("NPD_Census_2425_KtS_Autumn.txt", IngestRunStatus.Succeeded, null),
                ("NPD_Census_2425_KtS_Spring.txt", IngestRunStatus.Failed, "Processing failed."),
                ("NPD_Census_2425_KtS_Summer.txt", IngestRunStatus.Succeeded, null)
            ],
            (await dbContext.IngestRuns.OrderBy(r => r.DataFileName).ToListAsync()).Select(r => (r.DataFileName, r.Status, r.Error)));
    }

    [Theory]
    [InlineData("NPD_KS4_2025_U_KtS.txt", "Ks4 data files can't be uploaded to ks2.")]
    [InlineData("NPD_KS2_2025_U_KtS.xml", "'NPD_KS2_2025_U_KtS.xml' isn't a valid data file name. A data file must be one of .txt, .csv, .tsv.")]
    public async Task ProcessAsync_DataFileNotForFolder_FailsDataFile(string dataFileName, string expectedError)
    {
        await UploadZipAsync("ks2", Ks2ZipName, (dataFileName, "data"));

        await ScanAndProcessAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal(expectedError, run.Error);
        Assert.Equal(IngestFileStatus.Failed, (await dbContext.IngestFiles.SingleAsync()).Status);
    }

    [Fact]
    public async Task ProcessAsync_NoProcessorForDataset_FailsDataFile()
    {
        await UploadZipAsync("phonics", "NPD_PHONICS_2025_U.zip", ("NPD_PHONICS_2025_U_KtS.txt", "data"));

        await ScanAndProcessAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("Phonics data files can't be processed yet.", run.Error);
    }

    [Fact]
    public async Task ScanUploadsAsync_FileAlreadyQueued_IsNotQueuedAgain()
    {
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));

        await _scheduler.ScanUploadsAsync();
        await _scheduler.ScanUploadsAsync();

        Assert.Equal([Ks2FilePath], _backgroundJobClient.EnqueuedPaths);
    }

    [Fact]
    public async Task ScanUploadsAsync_UploadedAgainAfterSucceeding_MovesToErrorWithoutProcessing()
    {
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await ScanAndProcessAsync();

        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "new data"));
        await ScanAndProcessAsync();

        Assert.Single(_ks2Processor.Processed);
        Assert.Equal(
            [MovedBlobName("ks2/error/", Ks2ZipName, SecondTimestamp), MovedBlobName("ks2/processed/", Ks2ZipName, FirstTimestamp)],
            await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Succeeded, file.Status);
        Assert.Equal(1, file.Attempts);
    }

    [Fact]
    public async Task ScanUploadsAsync_UploadedAgainAfterFailing_ProcessesItAgain()
    {
        _ks2Processor.FailOn = Ks2DataFileName;
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await ScanAndProcessAsync();

        _ks2Processor.FailOn = null;
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "fixed data"));
        await ScanAndProcessAsync();

        Assert.Equal(["data", "fixed data"], _ks2Processor.Processed.Select(p => p.Content));
        Assert.Equal(
            [MovedBlobName("ks2/error/", Ks2ZipName, FirstTimestamp), MovedBlobName("ks2/processed/", Ks2ZipName, SecondTimestamp)],
            await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Succeeded, file.Status);
        Assert.Equal(2, file.Attempts);
        Assert.Equal(
            [(1, IngestRunStatus.Failed), (2, IngestRunStatus.Succeeded)],
            (await dbContext.IngestRuns.OrderBy(r => r.Attempt).ToListAsync()).Select(r => (r.Attempt, r.Status)));
    }

    [Fact]
    public async Task ProcessAsync_SameZipMovedTwice_KeepsBothCopies()
    {
        _ks2Processor.FailOn = Ks2DataFileName;
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await ScanAndProcessAsync();

        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "still broken"));
        await ScanAndProcessAsync();

        Assert.Equal(
            ["ks2/error/NPD_KS2_2025_U.20261120T101500Z.zip", "ks2/error/NPD_KS2_2025_U.20261120T101600Z.zip"],
            await ListBlobsAsync());
        Assert.Equal("data", await ReadZipEntryAsync("ks2/error/NPD_KS2_2025_U.20261120T101500Z.zip"));
        Assert.Equal("still broken", await ReadZipEntryAsync("ks2/error/NPD_KS2_2025_U.20261120T101600Z.zip"));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal("ks2/error/NPD_KS2_2025_U.20261120T101600Z.zip", (await dbContext.IngestFiles.SingleAsync()).BlobName);
    }

    [Fact]
    public async Task ProcessAsync_Replay_ProcessesFileAgainFromWhereItIs()
    {
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await ScanAndProcessAsync();

        await _processor.ProcessAsync(Ks2FilePath);

        Assert.Equal(2, _ks2Processor.Processed.Count);
        Assert.Equal([MovedBlobName("ks2/processed/", Ks2ZipName, FirstTimestamp)], await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Succeeded, file.Status);
        Assert.Equal(2, file.Attempts);
        Assert.Equal(2, await dbContext.IngestRuns.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ReplayOfFailedFileThatNowSucceeds_MovesToProcessed()
    {
        _ks2Processor.FailOn = Ks2DataFileName;
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await ScanAndProcessAsync();

        _ks2Processor.FailOn = null;
        await _processor.ProcessAsync(Ks2FilePath);

        Assert.Equal([MovedBlobName("ks2/processed/", Ks2ZipName, SecondTimestamp)], await ListBlobsAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Succeeded, file.Status);
        Assert.Null(file.Error);
    }

    [Fact]
    public async Task ProcessAsync_FailsBeforeMoving_LeavesFileWhereItIsAndDoesNotQueueItAgain()
    {
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await _scheduler.ScanUploadsAsync();

        await _containerClient.DeleteBlobAsync("ks2/upload/" + Ks2ZipName);
        await Assert.ThrowsAnyAsync<Exception>(() => _processor.ProcessAsync(Ks2FilePath));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestFile file = await dbContext.IngestFiles.SingleAsync();
        Assert.Equal(IngestFileStatus.Failed, file.Status);
        Assert.Equal("ks2/upload/" + Ks2ZipName, file.BlobName);
        Assert.NotNull(file.Error);

        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await _scheduler.ScanUploadsAsync();

        Assert.Equal([Ks2FilePath], _backgroundJobClient.EnqueuedPaths);
    }

    [Fact]
    public async Task RequeueFailedAsync_QueuesOnlyFailedFiles()
    {
        _ks2Processor.FailOn = Ks2DataFileName;
        await UploadZipAsync("ks2", Ks2ZipName, (Ks2DataFileName, "data"));
        await UploadZipAsync("census", "NPD_Census_2425.zip", ("NPD_Census_2425_KtS_Autumn.txt", "autumn"));
        await ScanAndProcessAsync();

        await _scheduler.RequeueFailedAsync();

        Assert.Equal([Ks2FilePath], _backgroundJobClient.EnqueuedPaths);

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        Assert.Equal(IngestFileStatus.Pending, (await dbContext.IngestFiles.SingleAsync(f => f.Path == Ks2FilePath)).Status);
    }

    // Runs a scan, then processes the files it enqueued, as the Worker would
    private async Task ScanAndProcessAsync()
    {
        await _scheduler.ScanUploadsAsync();

        foreach (string path in _backgroundJobClient.EnqueuedPaths)
        {
            try
            {
                await _processor.ProcessAsync(path);
            }
            catch (IngestFileFailedException)
            {
            }
        }

        _backgroundJobClient.EnqueuedPaths.Clear();
        _timeProvider.Advance(TimeSpan.FromMinutes(1));
    }

    private static string MovedBlobName(string folder, string zipName, string timestamp) =>
        folder + Path.GetFileNameWithoutExtension(zipName) + "." + timestamp + ".zip";

    private async Task UploadZipAsync(string folderName, string zipName, params (string Name, string Content)[] entries)
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
        await _containerClient.UploadBlobAsync($"{folderName}/upload/{zipName}", stream);
    }

    private Task UploadBlobAsync(string blobName, string content) =>
        _containerClient.UploadBlobAsync(blobName, BinaryData.FromString(content));

    private Task<List<string>> ListBlobsAsync() => ListBlobsAsync(_containerClient);

    private static async Task<List<string>> ListBlobsAsync(BlobContainerClient containerClient)
    {
        List<string> names = [];

        await foreach (Azure.Storage.Blobs.Models.BlobItem blob in containerClient.GetBlobsAsync())
        {
            names.Add(blob.Name);
        }

        return [.. names.Order(StringComparer.Ordinal)];
    }

    private async Task<string> ReadZipEntryAsync(string blobName)
    {
        using MemoryStream stream = new();
        await _containerClient.GetBlobClient(blobName).DownloadToAsync(stream);
        stream.Position = 0;

        using ZipArchive archive = new(stream, ZipArchiveMode.Read);
        using StreamReader reader = new(archive.Entries.Single().Open());

        return await reader.ReadToEndAsync();
    }

    private sealed class FakeDatasetProcessor(params IngestDataset[] datasets) : IIngestDatasetProcessor
    {
        public sealed record ProcessedFile(string Name, IngestDataFileAttributes Attributes, string Content);

        public IReadOnlyCollection<IngestDataset> Datasets { get; } = datasets;

        public List<ProcessedFile> Processed { get; } = [];

        public IngestDataFileResult Result { get; set; } = new(RecordsRead: 1, RecordsWritten: 1, RecordsRemoved: 0, RejectedRows: []);

        public string? FailOn { get; set; }

        public async Task<IngestDataFileResult> ProcessAsync(IngestDataFile file, CancellationToken cancellationToken)
        {
            using StreamReader reader = new(file.Content);
            Processed.Add(new ProcessedFile(file.Name, file.Attributes, await reader.ReadToEndAsync(cancellationToken)));

            return file.Name == FailOn ? throw new InvalidOperationException("Processing failed.") : Result;
        }
    }

    private sealed class RecordingBackgroundJobClient : IBackgroundJobClient
    {
        public List<string> EnqueuedPaths { get; } = [];

        public string Create(Job job, IState state)
        {
            EnqueuedPaths.Add((string)job.Args[0]);

            return EnqueuedPaths.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public bool ChangeState(string jobId, IState state, string expectedState) => throw new NotSupportedException();
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public void Advance(TimeSpan duration) => utcNow += duration;

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
