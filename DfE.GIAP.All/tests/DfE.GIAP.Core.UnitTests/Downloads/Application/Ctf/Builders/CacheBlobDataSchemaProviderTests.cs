using System.Text;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;
using DfE.GIAP.Core.Downloads.Application.Ctf.Options;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class CacheBlobDataSchemaProviderTests
{
    private static IOptions<CtfSchemaCacheOptions> CreateOptions(int minutes)
    {
        CtfSchemaCacheOptions options = new CtfSchemaCacheOptions
        {
            CacheDays = 0,
            CacheHours = 0,
            CacheMinutes = minutes
        };

        return Options.Create(options);
    }

    private static Stream CreateJsonStream(string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return new MemoryStream(bytes);
    }

    [Fact]
    public async Task GetAllSchemasAsync_LoadsSchemasOnFirstCall()
    {
        // Arrange
        Mock<IBlobStorageProvider> blobMock = new Mock<IBlobStorageProvider>();

        List<BlobItemMetadata> blobList = new List<BlobItemMetadata>
        {
            new BlobItemMetadata { Name = "schema1.json" }
        };

        blobMock
            .Setup(x => x.ListBlobsWithMetadataAsync(
                "giapdownloads",
                "CTF",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobList);

        DataSchemaDefinition schema = new DataSchemaDefinition { Year = "2023" };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(schema);

        blobMock
            .Setup(x => x.DownloadBlobAsStreamAsync(
                "giapdownloads",
                "schema1.json",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonStream(json));

        CacheBlobDataSchemaProvider provider =
            new CacheBlobDataSchemaProvider(blobMock.Object, CreateOptions(10));

        // Act
        IReadOnlyList<DataSchemaDefinition> result = await provider.GetAllSchemasAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("2023", result[0].Year);
        blobMock.Verify(x => x.ListBlobsWithMetadataAsync(
            "giapdownloads",
            "CTF",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllSchemasAsync_UsesCache_WhenNotExpired()
    {
        // Arrange
        Mock<IBlobStorageProvider> blobMock = new Mock<IBlobStorageProvider>();

        List<BlobItemMetadata> blobList = new List<BlobItemMetadata>
        {
            new BlobItemMetadata { Name = "schema.json" }
        };

        blobMock
            .Setup(x => x.ListBlobsWithMetadataAsync(
                "giapdownloads",
                "CTF",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobList);

        DataSchemaDefinition schema = new DataSchemaDefinition { Year = "2024" };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(schema);

        blobMock
            .Setup(x => x.DownloadBlobAsStreamAsync(
                "giapdownloads",
                "schema.json",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonStream(json));

        CacheBlobDataSchemaProvider provider =
            new CacheBlobDataSchemaProvider(blobMock.Object, CreateOptions(10));

        // First call loads cache
        IReadOnlyList<DataSchemaDefinition> first = await provider.GetAllSchemasAsync();

        // Act: second call should NOT hit blob storage
        IReadOnlyList<DataSchemaDefinition> second = await provider.GetAllSchemasAsync();

        // Assert
        Assert.Equal(first, second);
        blobMock.Verify(x => x.ListBlobsWithMetadataAsync(
            "giapdownloads",
            "CTF",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllSchemasAsync_RefreshesCache_WhenExpired()
    {
        // Arrange
        Mock<IBlobStorageProvider> blobMock = new Mock<IBlobStorageProvider>();

        List<BlobItemMetadata> blobList = new List<BlobItemMetadata>
        {
            new BlobItemMetadata { Name = "schema.json" }
        };

        blobMock
            .Setup(x => x.ListBlobsWithMetadataAsync(
                "giapdownloads",
                "CTF",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobList);

        DataSchemaDefinition schema1 = new DataSchemaDefinition { Year = "2021" };
        DataSchemaDefinition schema2 = new DataSchemaDefinition { Year = "2022" };

        string json1 = Newtonsoft.Json.JsonConvert.SerializeObject(schema1);
        string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(schema2);

        blobMock
            .SetupSequence(x => x.DownloadBlobAsStreamAsync(
                "giapdownloads",
                "schema.json",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonStream(json1))
            .ReturnsAsync(CreateJsonStream(json2));

        CacheBlobDataSchemaProvider provider =
            new CacheBlobDataSchemaProvider(blobMock.Object, CreateOptions(0));

        // Act
        IReadOnlyList<DataSchemaDefinition> first = await provider.GetAllSchemasAsync();
        IReadOnlyList<DataSchemaDefinition> second = await provider.GetAllSchemasAsync();

        // Assert
        Assert.Equal("2021", first[0].Year);
        Assert.Equal("2022", second[0].Year);

        blobMock.Verify(x => x.ListBlobsWithMetadataAsync(
            "giapdownloads",
            "CTF",
            It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetSchemaByYearAsync_ReturnsCorrectSchema()
    {
        // Arrange
        Mock<IBlobStorageProvider> blobMock = new Mock<IBlobStorageProvider>();

        List<BlobItemMetadata> blobList = new List<BlobItemMetadata>
        {
            new BlobItemMetadata { Name = "schema1.json" },
            new BlobItemMetadata { Name = "schema2.json" }
        };

        blobMock
            .Setup(x => x.ListBlobsWithMetadataAsync(
                "giapdownloads",
                "CTF",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobList);

        DataSchemaDefinition schema2020 = new DataSchemaDefinition { Year = "2020" };
        DataSchemaDefinition schema2021 = new DataSchemaDefinition { Year = "2021" };

        string json2020 = Newtonsoft.Json.JsonConvert.SerializeObject(schema2020);
        string json2021 = Newtonsoft.Json.JsonConvert.SerializeObject(schema2021);

        blobMock
            .Setup(x => x.DownloadBlobAsStreamAsync(
                "giapdownloads",
                "schema1.json",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonStream(json2020));

        blobMock
            .Setup(x => x.DownloadBlobAsStreamAsync(
                "giapdownloads",
                "schema2.json",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonStream(json2021));

        CacheBlobDataSchemaProvider provider =
            new CacheBlobDataSchemaProvider(blobMock.Object, CreateOptions(10));

        // Act
        DataSchemaDefinition? result = await provider.GetSchemaByYearAsync(2021);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2021", result.Year);
    }

    [Fact]
    public async Task GetSchemaByYearAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        Mock<IBlobStorageProvider> blobMock = new Mock<IBlobStorageProvider>();

        List<BlobItemMetadata> blobList = new List<BlobItemMetadata>
        {
            new BlobItemMetadata { Name = "schema.json" }
        };

        blobMock
            .Setup(x => x.ListBlobsWithMetadataAsync(
                "giapdownloads",
                "CTF",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobList);

        DataSchemaDefinition schema = new DataSchemaDefinition { Year = "2020" };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(schema);

        blobMock
            .Setup(x => x.DownloadBlobAsStreamAsync(
                "giapdownloads",
                "schema.json",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonStream(json));

        CacheBlobDataSchemaProvider provider =
            new CacheBlobDataSchemaProvider(blobMock.Object, CreateOptions(10));

        // Act
        DataSchemaDefinition? result = await provider.GetSchemaByYearAsync(1999);

        // Assert
        Assert.Null(result);
    }
}
