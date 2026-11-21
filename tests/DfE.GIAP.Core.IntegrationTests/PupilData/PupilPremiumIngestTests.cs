using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
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
public sealed class PupilPremiumIngestTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string Header =
        "PupilMatchingRef\tLA\tEstab\tURN\tLAEstab\tUPN\tSurname\tForename\tMiddlenames\tSex\tDOB\tNCYear\tSource\tAdopted\t" +
        "DeprivationPupilPremium\tLookedAfterPremium\tMODSERVICE\tCENSUSSERVICEEVER6\tPupilPremiumFTE\tPupilPremiumCashAmount\t" +
        "PupilPremiumFYStartDate\tPupilPremiumFYEndDate\tLastFSM";

    // The columns every prepared download ends with
    private const string PupilHeader =
        "UPN,Surname,Forename,Sex,DOB,NCYear,Deprivation Pupil Premium,Service Child Premium,Adopted from Care Premium," +
        "Looked After Premium,PupilPremiumFTE,PupilPremiumCashAmount,PupilPremiumFYStartDate,PupilPremiumFYEndDate,LastFSM";

    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobContainerClient _metadataContainerClient;
    private readonly BlobContainerClient _downloadsContainerClient;
    private readonly IngestFileProcessor _processor;

    public PupilPremiumIngestTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, IngestStorage.ContainerName);
        _metadataContainerClient = new BlobContainerClient(azurite.ConnectionString, IngestMetadataPublisher.ContainerName);
        _downloadsContainerClient = new BlobContainerClient(azurite.ConnectionString, PreparedDownloadPublisher.ContainerName);

        GiapPostgresDbContextFactory dbContextFactory = new(postgres);

        PupilPremiumDatasetProcessor datasetProcessor = new(
            new PupilPremiumDataWriter(dbContextFactory),
            new PupilPremiumPreparedDownloads(new PreparedDownloadWriter(
                dbContextFactory,
                new PreparedDownloadPublisher(_downloadsContainerClient),
                NullLogger<PreparedDownloadWriter>.Instance)));

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
        await dbContext.EstablishmentGroupLinks.ExecuteDeleteAsync();
        await dbContext.EstablishmentGroups.ExecuteDeleteAsync();
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
                Header + "\n" +
                Row("P1", middleNames: "Jo", dob: "01/02/2015") + "\n" +
                Row("P2", adopted: "1") + "\n" +
                Row("P1", la: "202", estab: "", urn: "", laEstab: "", middleNames: "Jo", dob: "01/02/2015", source: "A", adopted: "") + "\n"),
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

        JsonArray data = JsonNode.Parse(pupil.Data)!.AsArray();
        Assert.Equal(2, data.Count);
        Assert.Equal(
            ("201", "2015-02-01T00:00:00.000Z", "0", "C"),
            ((string?)data[0]!["LA"], (string?)data[0]!["DOB"], (string?)data[0]!["AdoptedfromCarePremium"], (string?)data[0]!["Source"]));
        Assert.Equal(
            ("202", null, null, "A"),
            ((string?)data[1]!["LA"], (string?)data[1]!["URN"], (string?)data[1]!["AdoptedfromCarePremium"], (string?)data[1]!["Source"]));

        Assert.True(await _metadataContainerClient.GetBlobClient("Pupil premium metadata.xlsx").ExistsAsync());
    }

    [Fact]
    public async Task ProcessAsync_PupilPremiumZip_WritesPreparedDownloads()
    {
        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            dbContext.EstablishmentGroups.AddRange(
                Group(1, EstablishmentGroup.MultiAcademyTrustGroupTypeCode),
                Group(2, EstablishmentGroup.SingleAcademyTrustGroupTypeCode));
            dbContext.EstablishmentGroupLinks.AddRange(
                new EstablishmentGroupLink { GroupUid = 1, Urn = 100000, JoinedDate = null },
                new EstablishmentGroupLink { GroupUid = 1, Urn = 100001, JoinedDate = null },
                new EstablishmentGroupLink { GroupUid = 2, Urn = 100001, JoinedDate = null });
            await dbContext.SaveChangesAsync();
        }

        await UploadAndProcessAsync(
            "NPD_pupil-premium_2425.zip",
            ("NPD_pupil-premium_2425_F.txt",
                Header + "\n" +
                // The school's service child premium includes MODSERVICE
                Row("P1", modService: "1", censusServiceEver6: "0") + "\n" +
                // Adopted pupils aren't loaded into Postgres, but they're in the prepared downloads
                Row("P2", upn: "B123456789012", adopted: "1", censusServiceEver6: "1", dob: "not a date") + "\n" +
                Row("P3", la: "202", estab: "3615", urn: "100001", laEstab: "2023615", upn: "C123456789012") + "\n" +
                Row("P4", estab: "", urn: "", laEstab: "", upn: "D123456789012", source: "A") + "\n" +
                // No LA, so it isn't in any LA file
                Row("P5", la: "", estab: "", urn: "", laEstab: "", upn: "E123456789012", source: "A") + "\n"));

        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            Assert.Equal(IngestFileStatus.Succeeded, (await dbContext.IngestFiles.SingleAsync()).Status);
        }

        Assert.Equal(
            [
                "LA/201/pp2425_201_2.csv",
                "LA/201/pp2425_201_AP_2.csv",
                "LA/202/pp2425_202_2.csv",
                "MAT/1/pp2425_1_2.csv",
                "SAT/2/pp2425_2_2.csv",
                "School/100000/pp2425_2013614_2.csv",
                "School/100001/pp2425_2023615_2.csv"
            ],
            await ListDownloadsAsync());

        Assert.Equal(
            $"LA,Estab,{PupilHeader}\n" +
            "201,3614,A123456789012,Smith,Alex,M,2015-02-01,5,1,0,0,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n" +
            "201,3614,B123456789012,Smith,Alex,M,,5,1,1,1,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n",
            await DownloadAsync("LA/201/pp2425_201_2.csv"));
        Assert.Equal(
            $"LA,AlternativeProvision,{PupilHeader}\n" +
            "201,ALT,D123456789012,Smith,Alex,M,2015-02-01,5,1,0,0,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n",
            await DownloadAsync("LA/201/pp2425_201_AP_2.csv"));
        Assert.Equal(
            $"{PupilHeader}\n" +
            "A123456789012,Smith,Alex,M,2015-02-01,5,1,1,0,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n" +
            "B123456789012,Smith,Alex,M,,5,1,1,1,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n",
            await DownloadAsync("School/100000/pp2425_2013614_2.csv"));
        Assert.Equal(
            $"GroupUID,LA,Estab,{PupilHeader}\n" +
            "1,201,3614,A123456789012,Smith,Alex,M,2015-02-01,5,1,0,0,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n" +
            "1,201,3614,B123456789012,Smith,Alex,M,,5,1,1,1,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n" +
            "1,202,3615,C123456789012,Smith,Alex,M,2015-02-01,5,1,0,0,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n",
            await DownloadAsync("MAT/1/pp2425_1_2.csv"));
        Assert.Equal(
            $"GroupUID,LA,Estab,{PupilHeader}\n" +
            "2,202,3615,C123456789012,Smith,Alex,M,2015-02-01,5,1,0,0,0,1.0,\"1,480.00\",2024-04-01,2025-03-31,1\n",
            await DownloadAsync("SAT/2/pp2425_2_2.csv"));
    }

    [Fact]
    public async Task ProcessAsync_FileMissingAPreparedDownloadColumn_FailsDataFileWithoutWritingPupils()
    {
        await UploadAndProcessAsync(
            "NPD_pupil-premium_2425.zip",
            ("NPD_pupil-premium_2425_U.txt", Header.Replace("\tLastFSM", "", StringComparison.Ordinal) + "\n" + Row("P1")[..Row("P1").LastIndexOf('\t')]));

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        IngestRun run = await dbContext.IngestRuns.SingleAsync();
        Assert.Equal(IngestRunStatus.Failed, run.Status);
        Assert.Equal("The data file is missing the following columns: 'LastFSM'.", run.Error);
        Assert.Empty(await dbContext.PupilPremiumPupils.ToListAsync());
        Assert.Empty(await ListDownloadsAsync());
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
        Assert.Equal(["LA/201/pp2425_201_2.csv", "School/100000/pp2425_2013614_2.csv"], await ListDownloadsAsync());
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

    private static string Row(
        string pupilMatchingRef,
        string la = "201",
        string estab = "3614",
        string urn = "100000",
        string laEstab = "2013614",
        string upn = "A123456789012",
        string middleNames = "",
        string dob = "2015-02-01",
        string source = "C",
        string adopted = "0",
        string modService = "0",
        string censusServiceEver6 = "0") =>
        string.Join(
            '\t',
            pupilMatchingRef, la, estab, urn, laEstab, upn, "Smith", "Alex", middleNames, "M", dob, "5", source, adopted,
            "1", "0", modService, censusServiceEver6, "1.0", "1,480.00", "01/04/2024", "2025-03-31", "1");

    private static EstablishmentGroup Group(int groupUid, int groupTypeCode) => new()
    {
        GroupUid = groupUid,
        GroupId = null,
        Name = $"Group {groupUid}",
        GroupTypeCode = groupTypeCode,
        GroupTypeName = "Trust",
        GroupStatusCode = EstablishmentGroup.OpenGroupStatusCode,
        GroupStatusName = "Open",
        ClosedDate = null
    };

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
