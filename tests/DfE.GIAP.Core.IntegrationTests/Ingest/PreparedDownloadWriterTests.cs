using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.IntegrationTests.TestHarness;
using DfE.GIAP.SharedTests.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DfE.GIAP.Core.IntegrationTests.Ingest;

[Collection(PostgresIntegrationTestCollectionMarker.Name)]
public sealed class PreparedDownloadWriterTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
{
    private readonly GiapPostgresFixture _postgres;
    private readonly BlobContainerClient _containerClient;
    private readonly PreparedDownloadWriter _writer;

    public PreparedDownloadWriterTests(GiapPostgresFixture postgres, AzuriteFixture azurite)
    {
        _postgres = postgres;
        _containerClient = new BlobContainerClient(azurite.ConnectionString, PreparedDownloadPublisher.ContainerName);
        _writer = new PreparedDownloadWriter(
            new GiapPostgresDbContextFactory(postgres),
            new PreparedDownloadPublisher(_containerClient),
            NullLogger<PreparedDownloadWriter>.Instance);
    }

    public async Task InitializeAsync()
    {
        await _containerClient.DeleteIfExistsAsync();
        await _containerClient.CreateAsync();

        await using GiapDbContext dbContext = _postgres.CreateDbContext();
        await dbContext.EstablishmentGroupLinks.ExecuteDeleteAsync();
        await dbContext.EstablishmentGroups.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task WriteAsync_WritesAFilePerKeyInRowOrder()
    {
        PreparedDownloadAudience[] audiences = [new("LA", "LA", "file_*_1", ["LA", "Name", "Note"])];

        int filesWritten = await _writer.WriteAsync(
            audiences,
            Rows(
                new PreparedDownloadRow(0, "202", "202", 3, ["202", "C", null]),
                new PreparedDownloadRow(0, "201", "201", 2, ["201", "B", "has, a comma"]),
                new PreparedDownloadRow(0, "201", "201", 1, ["201", "A", "has \"quotes\""]),
                new PreparedDownloadRow(0, null, null, 4, [null, "D", null])));

        Assert.Equal(2, filesWritten);
        Assert.Equal(["LA/201/file_201_1.csv", "LA/202/file_202_1.csv"], await ListBlobsAsync());
        Assert.Equal(
            "LA,Name,Note\n201,A,\"has \"\"quotes\"\"\"\n201,B,\"has, a comma\"\n",
            await DownloadAsync("LA/201/file_201_1.csv"));
        Assert.Equal("LA,Name,Note\n202,C,\n", await DownloadAsync("LA/202/file_202_1.csv"));

        BlobProperties properties = await _containerClient.GetBlobClient("LA/202/file_202_1.csv").GetPropertiesAsync();
        Assert.Equal("text/csv", properties.ContentType);
    }

    [Fact]
    public async Task WriteAsync_NamesFilesByTheirFirstRowAndSkipsFilesWithoutAName()
    {
        PreparedDownloadAudience[] audiences = [new("School", "School", "file_*", ["Name"])];

        await _writer.WriteAsync(
            audiences,
            Rows(
                new PreparedDownloadRow(0, "100000", "2013614", 1, ["A"]),
                new PreparedDownloadRow(0, "100000", "9999999", 2, ["B"]),
                new PreparedDownloadRow(0, "100001", null, 3, ["C"]),
                new PreparedDownloadRow(0, "100001", "2013615", 4, ["D"])));

        Assert.Equal(["School/100000/file_2013614.csv"], await ListBlobsAsync());
        Assert.Equal("Name\nA\nB\n", await DownloadAsync("School/100000/file_2013614.csv"));
    }

    [Fact]
    public async Task WriteAsync_GroupAudience_WritesRowsToEveryOpenGroupOfTheType()
    {
        await using (GiapDbContext dbContext = _postgres.CreateDbContext())
        {
            dbContext.EstablishmentGroups.AddRange(
                Group(1, EstablishmentGroup.MultiAcademyTrustGroupTypeCode, "OPEN"),
                Group(2, EstablishmentGroup.MultiAcademyTrustGroupTypeCode, "OPEN"),
                Group(3, EstablishmentGroup.MultiAcademyTrustGroupTypeCode, "CLOSED"),
                Group(4, EstablishmentGroup.SingleAcademyTrustGroupTypeCode, "OPEN"));
            dbContext.EstablishmentGroupLinks.AddRange(
                Link(1, 100000),
                Link(2, 100000),
                Link(2, 100001),
                Link(3, 100001),
                Link(4, 100001));
            await dbContext.SaveChangesAsync();
        }

        PreparedDownloadAudience[] audiences =
            [new("MAT", "MAT", "file_*", ["GroupUID", "Name"], EstablishmentGroup.MultiAcademyTrustGroupTypeCode)];

        int filesWritten = await _writer.WriteAsync(
            audiences,
            Rows(
                new PreparedDownloadRow(0, "100001", null, 1, ["A"]),
                new PreparedDownloadRow(0, "100000", null, 2, ["B"]),
                new PreparedDownloadRow(0, "100002", null, 3, ["C"])));

        Assert.Equal(2, filesWritten);
        Assert.Equal(["MAT/1/file_1.csv", "MAT/2/file_2.csv"], await ListBlobsAsync());
        Assert.Equal("GroupUID,Name\n1,B\n", await DownloadAsync("MAT/1/file_1.csv"));
        Assert.Equal("GroupUID,Name\n2,A\n2,B\n", await DownloadAsync("MAT/2/file_2.csv"));
    }

    [Fact]
    public async Task WriteAsync_FileAlreadyExists_ReplacesIt()
    {
        await _containerClient.UploadBlobAsync("LA/201/file_201.csv", BinaryData.FromString("old"));
        await _containerClient.UploadBlobAsync("LA/201/older_201.csv", BinaryData.FromString("old"));

        await _writer.WriteAsync(
            [new("LA", "LA", "file_*", ["LA"])],
            Rows(new PreparedDownloadRow(0, "201", "201", 1, ["201"])));

        Assert.Equal("LA\n201\n", await DownloadAsync("LA/201/file_201.csv"));
        Assert.Equal("old", await DownloadAsync("LA/201/older_201.csv"));
    }

    [Theory]
    [InlineData(new[] { "a", "b" }, "a,b")]
    [InlineData(new[] { "", null }, ",")]
    [InlineData(new[] { "line\nbreak", "carriage\rreturn" }, "\"line\nbreak\",\"carriage\rreturn\"")]
    public void FormatLine(string?[] values, string expected) =>
        Assert.Equal(expected, PreparedDownloadWriter.FormatLine(values));

    private static EstablishmentGroup Group(int groupUid, int groupTypeCode, string groupStatusCode) => new()
    {
        GroupUid = groupUid,
        GroupId = null,
        Name = $"Group {groupUid}",
        GroupTypeCode = groupTypeCode,
        GroupTypeName = "Trust",
        GroupStatusCode = groupStatusCode,
        GroupStatusName = groupStatusCode,
        ClosedDate = null
    };

    private static EstablishmentGroupLink Link(int groupUid, int urn) => new()
    {
        GroupUid = groupUid,
        Urn = urn,
        JoinedDate = null
    };

    private static async IAsyncEnumerable<PreparedDownloadRow> Rows(params PreparedDownloadRow[] rows)
    {
        foreach (PreparedDownloadRow row in rows)
        {
            await Task.Yield();
            yield return row;
        }
    }

    private async Task<List<string>> ListBlobsAsync()
    {
        List<string> names = [];

        await foreach (BlobItem blob in _containerClient.GetBlobsAsync())
        {
            names.Add(blob.Name);
        }

        return names;
    }

    private async Task<string> DownloadAsync(string blobName)
    {
        Response<BlobDownloadResult> response = await _containerClient.GetBlobClient(blobName).DownloadContentAsync();
        return response.Value.Content.ToString();
    }
}
