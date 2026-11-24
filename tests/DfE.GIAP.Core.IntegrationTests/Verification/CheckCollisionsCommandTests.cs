using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.SharedTests.Postgres;
using DfE.GIAP.Verification;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Verification;

/// <summary>
/// Finds the keys in the anonymised zips that are already in test, in Postgres or Cosmos DB.
/// </summary>
[Collection(CosmosDbIntegrationTestCollectionMarker.Name)]
public sealed class CheckCollisionsCommandTests : IClassFixture<GiapPostgresFixture>, IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private readonly GiapCosmosDbFixture _cosmos;
    private readonly GiapPostgresFixture _postgres;
    private readonly AzuriteFixture _azurite;

    public CheckCollisionsCommandTests(GiapCosmosDbFixture cosmos, GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _cosmos = cosmos;
        _postgres = postgres;
        _azurite = azurite;
    }

    public async Task InitializeAsync()
    {
        await _cosmos.InvokeAsync(_cosmos.DatabaseName, client => client.ClearDatabaseAsync());

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        await dbContext.NpdPupilRecords.ExecuteDeleteAsync();
        await dbContext.NpdPupils.ExecuteDeleteAsync();

        BlobContainerClient files = new(_azurite.ConnectionString, "verification");
        await files.CreateIfNotExistsAsync();
        await files.GetBlobClient("files/manifest.json").UploadAsync(
            BinaryData.FromString(
                """
                {
                    "files": [
                        { "source": "ks2/processed/NPD_KS2_2025_U.zip", "target": "ks2/NPD_KS2_2025_U.zip" },
                        { "source": "fe_pp/processed/FE_PP_2025.zip", "target": "fe_pp/FE_PP_2025.zip" }
                    ]
                }
                """),
            overwrite: true);
        await files.GetBlobClient("files/ks2/NPD_KS2_2025_U.zip").UploadAsync(
            Zip("NPD_KS2_2025_U_KtS.txt", "PupilMatchingRef\tSURNAME\nAB00000000001\tSmith\nAB00000000002\tJones\n"),
            overwrite: true);
        await files.GetBlobClient("files/fe_pp/FE_PP_2025.zip").UploadAsync(
            Zip("FE_PP_2025.txt", "ULN\tSURNAME\n1234567899\tSmith\n9876543210\tJones\n"),
            overwrite: true);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RunAsync_KeysAlreadyInTest_AreCollisions()
    {
        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            dbContext.NpdPupils.Add(new NpdPupil
            {
                PupilMatchingRef = "AB00000000002",
                Upn = null,
                LaCode = null,
                EstablishmentNumber = null,
                Urn = null,
                Surname = null,
                Forename = null,
                MiddleNames = null,
                Sex = null,
                Dob = null
            });
            await dbContext.SaveChangesAsync();
        }

        using (CosmosClient client = new(_cosmos.Connection.Endpoint, _cosmos.Connection.Key, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway }))
        {
            using MemoryStream document = new(Encoding.UTF8.GetBytes("""{ "id": "1234567899", "ULN": "1234567899" }"""));
            using ResponseMessage response = await client.GetDatabase(_cosmos.DatabaseName).GetContainer("further-education-v2")
                .CreateItemStreamAsync(document, new PartitionKey("1234567899"));
            response.EnsureSuccessStatusCode();
        }

        StringWriter output = new();
        StringWriter log = new();
        int exitCode = await CheckCollisionsCommand.RunAsync(Configuration(), output, log);

        Assert.True(exitCode == CompareCommand.DifferencesExitCode, output.ToString() + log);
        Assert.Contains("| pupil-noskill | 2 | 1 | 0 |", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("| further-education | 2 | 0 | 1 |", output.ToString(), StringComparison.Ordinal);

        BinaryData collisions = (await new BlobContainerClient(_azurite.ConnectionString, "verification")
            .GetBlobClient("files/collisions.json").DownloadContentAsync()).Value.Content;
        Assert.Equal(
            new Dictionary<string, string[]> { ["pupil-noskill"] = ["AB00000000002"], ["further-education"] = ["1234567899"] },
            JsonSerializer.Deserialize<Dictionary<string, string[]>>(collisions.ToString()));
    }

    [Fact]
    public async Task RunAsync_NoKeysAlreadyInTest_IsClean()
    {
        int exitCode = await CheckCollisionsCommand.RunAsync(Configuration(), new StringWriter(), new StringWriter());

        Assert.Equal(CompareCommand.CleanExitCode, exitCode);
    }

    private static BinaryData Zip(string name, string content)
    {
        using MemoryStream zip = new();
        using (ZipArchive archive = new(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            using Stream entry = archive.CreateEntry(name).Open();
            entry.Write(Encoding.UTF8.GetBytes(content));
        }

        return BinaryData.FromBytes(zip.ToArray());
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
                ["RepositoryOptions:Containers:reference:ContainerName"] = "reference"
            })
            .Build();
}
