using System.IO.Compression;
using System.Text;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Core.PupilData;
using DfE.GIAP.SharedTests.Postgres;
using DfE.GIAP.Verification;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace DfE.GIAP.Core.IntegrationTests.Verification;

/// <summary>
/// Runs the verification tool against a data file processed by the new pipeline, and what the old pipeline would have
/// written for it. Telling the differences apart is covered by the tool's unit tests; this covers reading each side.
/// </summary>
[Collection(CosmosDbIntegrationTestCollectionMarker.Name)]
public sealed class CompareCommandTests : IClassFixture<GiapPostgresFixture>, IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string FilePath = "ks2/NPD_KS2_2025_U.zip";
    private const string DataFileName = "NPD_KS2_2025_U_KtS.txt";
    private const string DownloadPath = "LA/201/ks2_2025_cla.csv";

    private readonly GiapCosmosDbFixture _cosmos;
    private readonly GiapPostgresFixture _postgres;
    private readonly AzuriteFixture _azurite;

    public CompareCommandTests(GiapCosmosDbFixture cosmos, GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _cosmos = cosmos;
        _postgres = postgres;
        _azurite = azurite;
    }

    public async Task InitializeAsync()
    {
        await _cosmos.InvokeAsync(_cosmos.DatabaseName, client => client.ClearDatabaseAsync());

        foreach (string container in new[] { IngestStorage.ContainerName, IngestMetadataPublisher.ContainerName, PreparedDownloadPublisher.ContainerName, "giapdownloads", "giapdataprocessing" })
        {
            await new BlobContainerClient(_azurite.ConnectionString, container).CreateIfNotExistsAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RunAsync_BothPipelinesWroteTheSame_IsClean()
    {
        await ProcessWithNewPipelineAsync(
            """
            PupilMatchingRef	LA	ESTAB	URN	UPN	SURNAME	FORENAMES	SEX	DOB	ACADYR	KS2_TOTMARK
            P1	201	3614	100000	A123456789012	Smith	Alex	M	01/02/2014	2024/2025	101
            P2	201	3614	100000	B123456789012	Jones	Sam	F	31/02/2014	2024/2025	99
            """);

        await WriteAsOldPipelineAsync();

        StringWriter output = new();
        StringWriter log = new();
        int exitCode = await CompareCommand.RunAsync(
            new CompareOptions(
                CompareScope.Keys,
                [FilePath],
                new Dictionary<string, string> { [DataFileName] = "run1" },
                new HashSet<Check>([Check.Records, Check.DataFiles, Check.Downloads]),
                Detail: true,
                BatchSize: 100,
                Concurrency: 2,
                MaxRequestUnits: null,
                OutputDirectory: null,
                RulesPath: null),
            Configuration(),
            output,
            log);

        Assert.True(exitCode == CompareCommand.CleanExitCode, output.ToString() + log);
        Assert.Contains("| pupil-noskill | 2 |", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("| file-results | 1 |", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("| rejected-rows | 1 |", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("| downloads | 1 |", output.ToString(), StringComparison.Ordinal);
    }

    private async Task ProcessWithNewPipelineAsync(string dataFile)
    {
        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            await dbContext.IngestRejectedRows.ExecuteDeleteAsync();
            await dbContext.IngestRuns.ExecuteDeleteAsync();
            await dbContext.IngestFiles.ExecuteDeleteAsync();
            await dbContext.NpdPupilRecords.ExecuteDeleteAsync();
            await dbContext.NpdPupils.ExecuteDeleteAsync();

            dbContext.IngestFiles.Add(new IngestFile
            {
                Path = FilePath,
                BlobName = "ks2/upload/NPD_KS2_2025_U.zip",
                Status = IngestFileStatus.Pending,
                FirstSeenAt = DateTime.UtcNow.AddMinutes(-1)
            });
            await dbContext.SaveChangesAsync();
        }

        using MemoryStream zip = new();
        using (ZipArchive archive = new(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            await using Stream entry = archive.CreateEntry(DataFileName).Open();
            await entry.WriteAsync(Encoding.UTF8.GetBytes(dataFile));
        }

        zip.Position = 0;
        BlobContainerClient ingestContainer = new(_azurite.ConnectionString, IngestStorage.ContainerName);
        await ingestContainer.GetBlobClient("ks2/upload/NPD_KS2_2025_U.zip").UploadAsync(zip, overwrite: true);

        GiapPostgresDbContextFactory dbContextFactory = new(_postgres);
        IngestFileProcessor processor = new(
            new IngestStorage(ingestContainer, TimeProvider.System),
            new IngestDatasetProcessorRegistry([new NpdDatasetProcessor(new NpdDataWriter(dbContextFactory))]),
            new IngestMetadataPublisher(new BlobContainerClient(_azurite.ConnectionString, IngestMetadataPublisher.ContainerName)),
            dbContextFactory,
            TimeProvider.System,
            NullLogger<IngestFileProcessor>.Instance);
        await processor.ProcessAsync(FilePath);

        await new BlobContainerClient(_azurite.ConnectionString, PreparedDownloadPublisher.ContainerName)
            .GetBlobClient(DownloadPath)
            .UploadAsync(BinaryData.FromString("CLA_LA,CLA_UPN\n201,A123456789012\n"), overwrite: true);
    }

    // What the old pipeline writes for the same data file, as far as the tool reads it
    private async Task WriteAsOldPipelineAsync()
    {
        using CosmosClient client = new(
            _cosmos.Connection.Endpoint,
            _cosmos.Connection.Key,
            new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
        Database database = client.GetDatabase(_cosmos.DatabaseName);

        await CreateItemAsync(
            database.GetContainer("pupil-noskill"),
            "P1",
            """
            {
                "id": "P1",
                "PupilMatchingRef": "P1",
                "UPN": "A123456789012",
                "LA": 201,
                "Estab": 3614,
                "URN": 100000,
                "Surname": "Smith",
                "Forename": "Alex",
                "Middlenames": null,
                "Sex": "M",
                "DOB": "2014-02-01",
                "KS2": [
                    {"PupilMatchingRef": "P1", "LA": "201", "ESTAB": "3614", "URN": "100000", "UPN": "A123456789012", "SURNAME": "Smith", "FORENAMES": "Alex", "SEX": "M", "DOB": "2014-02-01T00:00:00.000Z", "ACADYR": "2024/2025", "KS2_TOTMARK": "101"}
                ]
            }
            """);

        await CreateItemAsync(
            database.GetContainer("reference"),
            "KS2",
            $$"""
            {
                "id": "20250102030405",
                "DOCTYPE": "KS2",
                "File Name": "{{DataFileName}}",
                "Number Of Source Records": 2,
                "Number Of Failed QC Records": 1,
                "Number Of Passed QC Records": 1,
                "Number Of Unique Records": 1
            }
            """);

        await new BlobContainerClient(_azurite.ConnectionString, "giapdataprocessing")
            .GetBlobClient("ks2/report/giap_exceptions_run1.csv")
            .UploadAsync(
                BinaryData.FromString(
                    ",PupilMatchingRef,LA,ESTAB,URN,UPN,SURNAME,FORENAMES,SEX,DOB,ACADYR,KS2_TOTMARK\n" +
                    "1,P2,201,3614,100000,B123456789012,Jones,Sam,F,31/02/2014,2024/2025,99\n"),
                overwrite: true);

        await new BlobContainerClient(_azurite.ConnectionString, "giapdownloads")
            .GetBlobClient(DownloadPath)
            .UploadAsync(BinaryData.FromString("CLA_LA,CLA_UPN\n201,A123456789012\n"), overwrite: true);
    }

    private static async Task CreateItemAsync(Container container, string partitionKey, string json)
    {
        using MemoryStream content = new(Encoding.UTF8.GetBytes(json));
        using ResponseMessage response = await container.CreateItemStreamAsync(content, new PartitionKey(partitionKey));
        response.EnsureSuccessStatusCode();
    }

    private IConfiguration Configuration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{GiapDbContext.ConnectionName}"] = _postgres.ConnectionString,
                [$"ConnectionStrings:{IngestStorage.ConnectionName}"] = _azurite.ConnectionString,
                ["RepositoryOptions:EndpointUri"] = _cosmos.Connection.Endpoint,
                ["RepositoryOptions:PrimaryKey"] = _cosmos.Connection.Key,
                ["RepositoryOptions:DatabaseId"] = _cosmos.DatabaseName,
                ["RepositoryOptions:ConnectionMode"] = "Gateway",
                ["RepositoryOptions:Containers:pupil-noskill:ContainerName"] = "pupil-noskill",
                ["RepositoryOptions:Containers:pupil-premium:ContainerName"] = "pupil-premium-v2",
                ["RepositoryOptions:Containers:further-education:ContainerName"] = "further-education-v2",
                ["RepositoryOptions:Containers:reference:ContainerName"] = "reference",
                [CompareCommand.LegacyDownloadsStorageKey] = _azurite.ConnectionString,
                ["Verification:LegacyProcessingStorage"] = _azurite.ConnectionString
            })
            .Build();
}
