using System.IO.Compression;
using System.Text;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.Verification;
using DfE.GIAP.Verification.Anonymisation;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Verification;

/// <summary>
/// Anonymises a zip from the old pipeline's storage into the <c>verification</c> container, with the shipped
/// classification, and checks none of the real values are anywhere in what's written.
/// </summary>
public sealed class AnonymiseCommandTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private const string SourcePath = "ks2/processed/NPD_KS2_2025_U.zip";
    private const string TargetPath = "ks2/NPD_KS2_2025_U.zip";

    // Shaped like the old pipeline's KS2 test data
    private const string DataFile =
        "ACADYR\tPupilMatchingRef\tCANDNO\tCAND_ID\tUPN\tSURNAME\tFORENAMES\tDOB\tGENDER\tLA\tESTAB\tURN\tREADSCORE\r\n" +
        "2024/2025\tM891313215015\t552210\t484008\tP000000000001\tGilby\tSkylar\t20/10/2014\tM\t937\t4097\t112404\t105\r\n" +
        "2024/2025\tT304330217051\t210830\t183572\tP00000000000B\tLangsdon-Telling\tJohn Eddy\t2014-05-06\tF\t937\t4097\t112404\t98\r\n" +
        "2024/2025\t\t508793\t445397\tX373702308010\tWhitlock\tPriya\t31/02/2014\tF\t937\t4097\t112404\t\r\n";

    private static readonly string[] _realIdentifiers =
    [
        "M891313215015", "T304330217051", "552210", "484008", "210830", "183572", "508793", "445397",
        "P000000000001", "P00000000000B", "X373702308010", "20/10/2014", "2014-05-06"
    ];

    // Compared with whole words, as a fake name can contain a real one, e.g. Johnson
    private static readonly string[] _realNames = ["Gilby", "Skylar", "Langsdon", "Telling", "John", "Eddy", "Whitlock", "Priya"];

    private readonly AzuriteFixture _azurite;
    private readonly string _manifestPath = Path.GetTempFileName();

    public AnonymiseCommandTests(AzuriteFixture azurite)
    {
        _azurite = azurite;
    }

    public async Task InitializeAsync()
    {
        BlobContainerClient source = new(_azurite.ConnectionString, AnonymisationStorage.SourceContainerName);
        await source.CreateIfNotExistsAsync();
        await new BlobContainerClient(_azurite.ConnectionString, AnonymisationStorage.DestinationContainerName).CreateIfNotExistsAsync();

        using MemoryStream zip = new();
        using (ZipArchive archive = new(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            await using (Stream entry = archive.CreateEntry("NPD_KS2_2025_U_KtS.txt").Open())
            {
                await entry.WriteAsync(Encoding.UTF8.GetBytes(DataFile));
            }

            await using (Stream entry = archive.CreateEntry("KS2 guidance.docx").Open())
            {
                await entry.WriteAsync(Encoding.UTF8.GetBytes("Guidance"));
            }
        }

        zip.Position = 0;
        await source.GetBlobClient(SourcePath).UploadAsync(zip, overwrite: true);

        await File.WriteAllTextAsync(_manifestPath, $$"""{ "files": [{ "source": "{{SourcePath}}", "target": "{{TargetPath}}" }] }""");
    }

    public Task DisposeAsync()
    {
        File.Delete(_manifestPath);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RunAsync_WritesTheZipAnonymised_WithNoRealValuesInIt()
    {
        StringWriter log = new();
        int exitCode = await AnonymiseCommand.RunAsync(Options(overwrite: true), Configuration(), new StringWriter(), log);

        Assert.True(exitCode == CompareCommand.CleanExitCode, log.ToString());

        BlobContainerClient destination = new(_azurite.ConnectionString, AnonymisationStorage.DestinationContainerName);
        BinaryData anonymised = (await destination.GetBlobClient($"files/{TargetPath}").DownloadContentAsync()).Value.Content;

        using ZipArchive archive = new(anonymised.ToStream(), ZipArchiveMode.Read);
        Assert.Equal(["NPD_KS2_2025_U_KtS.txt", "KS2 guidance.docx"], archive.Entries.Select(e => e.FullName));

        string dataFile;
        using (StreamReader reader = new(archive.GetEntry("NPD_KS2_2025_U_KtS.txt")!.Open()))
        {
            dataFile = await reader.ReadToEndAsync();
        }

        AssertHasNoRealValues(dataFile);

        string[] lines = dataFile.Split("\r\n");
        Assert.Equal(DataFile.Split("\r\n")[0], lines[0]);
        Assert.Equal(5, lines.Length);
        // The rows still have what makes them rejected: no PMR, and a DOB that isn't a date
        Assert.StartsWith("2024/2025\t\t", lines[3], StringComparison.Ordinal);
        Assert.Contains("\t31/02/2014\t", lines[3], StringComparison.Ordinal);
        Assert.EndsWith("\t937\t4097\t112404\t105", lines[1], StringComparison.Ordinal);

        string manifest = (await destination.GetBlobClient("files/manifest.json").DownloadContentAsync()).Value.Content.ToString();
        Assert.Equal([TargetPath], AnonymisationManifest.Parse(manifest).Files.Select(f => f.Target));
    }

    [Fact]
    public async Task RunAsync_Twice_IsTheSame_AndLeavesWhatsThereWithoutOverwrite()
    {
        BlobContainerClient destination = new(_azurite.ConnectionString, AnonymisationStorage.DestinationContainerName);

        await AnonymiseCommand.RunAsync(Options(overwrite: true), Configuration(), new StringWriter(), new StringWriter());
        BinaryData first = (await destination.GetBlobClient($"files/{TargetPath}").DownloadContentAsync()).Value.Content;

        StringWriter log = new();
        int exitCode = await AnonymiseCommand.RunAsync(Options(overwrite: false), Configuration(), new StringWriter(), log);

        Assert.Equal(CompareCommand.CleanExitCode, exitCode);
        Assert.Contains("already there", log.ToString(), StringComparison.Ordinal);
        Assert.Equal(DataFileContent(first), DataFileContent((await destination.GetBlobClient($"files/{TargetPath}").DownloadContentAsync()).Value.Content));
    }

    [Fact]
    public async Task RunAsync_ListColumns_DescribesTheFile_WithoutItsValues()
    {
        StringWriter output = new();
        int exitCode = await AnonymiseCommand.RunAsync(Options(overwrite: false) with { ListColumns = true }, Configuration(), output, new StringWriter());

        Assert.Equal(CompareCommand.CleanExitCode, exitCode);
        Assert.Contains("`PupilMatchingRef`: Pmr", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("shape `A999999999999`: 2", output.ToString(), StringComparison.Ordinal);
        AssertHasNoRealValues(output.ToString());
    }

    [Fact]
    public async Task RunAsync_AKeyFromConfiguration_WithRealStorage_Fails()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddConfiguration(Configuration())
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [AnonymisationStorage.DestinationConfigurationKey] = "https://s189t01giaptsingestsa.blob.core.windows.net/verification?sig=x"
            })
            .Build();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AnonymiseCommand.RunAsync(Options(overwrite: true), configuration, new StringWriter(), new StringWriter()));
    }

    private static void AssertHasNoRealValues(string text)
    {
        foreach (string identifier in _realIdentifiers)
        {
            Assert.DoesNotContain(identifier, text, StringComparison.Ordinal);
        }

        string[] words = text.Split(['\t', '\r', '\n', ' ', '-', '`'], StringSplitOptions.RemoveEmptyEntries);
        foreach (string name in _realNames)
        {
            Assert.DoesNotContain(name, words, StringComparer.OrdinalIgnoreCase);
        }
    }

    private AnonymiseOptions Options(bool overwrite) => new(_manifestPath, KeyVaultName: null, ListColumns: false, overwrite, ClassificationPath: null);

    private static string DataFileContent(BinaryData zip)
    {
        using ZipArchive archive = new(zip.ToStream(), ZipArchiveMode.Read);
        using StreamReader reader = new(archive.GetEntry("NPD_KS2_2025_U_KtS.txt")!.Open());
        return reader.ReadToEnd();
    }

    private IConfiguration Configuration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Verification:LegacyProcessingStorage"] = _azurite.ConnectionString,
                [AnonymisationStorage.DestinationConfigurationKey] = _azurite.ConnectionString,
                [AnonymisationStorage.KeyConfigurationKey] = Convert.ToBase64String([.. Enumerable.Range(0, 32).Select(i => (byte)i)])
            })
            .Build();
}
