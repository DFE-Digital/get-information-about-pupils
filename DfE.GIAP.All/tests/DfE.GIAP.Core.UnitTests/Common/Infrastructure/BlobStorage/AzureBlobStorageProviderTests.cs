using System.Collections.ObjectModel;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Common.Infrastructure.BlobStorage;
public sealed class AzureBlobStorageProviderTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBlobServiceClientIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new AzureBlobStorageProvider(null!));
    }

    [Fact]
    public async Task ListBlobsByNamesAsync_Returns_ListOfBlobNames()
    {
        List<BlobItem> blobItems = new()
        {
            BlobItemTestDouble.CreateItemWith(name: "blob1.txt"),
            BlobItemTestDouble.CreateItemWith(name: "blob2.txt"),
        };

        Mock<BlobContainerClient> mockContainerClient = new();
        mockContainerClient
            .Setup(x => x.GetBlobsAsync(
                It.IsAny<BlobTraits>(),
                It.IsAny<BlobStates>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(BlobItemTestDouble.AsyncPageableItemsFrom(blobItems));

        Mock<BlobServiceClient> mockServiceClient = new();
        mockServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        AzureBlobStorageProvider sut = new AzureBlobStorageProvider(mockServiceClient.Object);

        IEnumerable<string> result = await sut.ListBlobsByNamesAsync(It.IsAny<string>(), It.IsAny<string>());

        Assert.Contains("blob1.txt", result);
        Assert.Contains("blob2.txt", result);
    }

    [Fact]
    public async Task ListBlobsWithMetadataAsync_Returns_Metadata()
    {
        List<BlobItem> blobItems = new()
        {
             BlobItemTestDouble.CreateItemWith(name: "blob1.txt", contentType: "text/plain")
        };

        Mock<BlobContainerClient> mockContainerClient = new();
        mockContainerClient
            .Setup(x => x.GetBlobsAsync(
                It.IsAny<BlobTraits>(),
                It.IsAny<BlobStates>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(BlobItemTestDouble.AsyncPageableItemsFrom(blobItems));

        Mock<BlobServiceClient> mockServiceClient = new();
        mockServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        AzureBlobStorageProvider sut = new(
            blobServiceClient: mockServiceClient.Object);

        IEnumerable<BlobItemMetadata> result = await sut.ListBlobsWithMetadataAsync(It.IsAny<string>(), It.IsAny<string>());

        BlobItemMetadata metadata = Assert.Single(result);
        Assert.Equal("blob1.txt", metadata.Name);
        Assert.Equal("text/plain", metadata.ContentType);
    }

    [Fact]
    public async Task BlobExistsAsync_ReturnsTrue_WhenBlobExists()
    {
        Mock<BlobClient> mockBlobClient = new();
        mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        Mock<BlobContainerClient> mockContainerClient = new();
        mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        Mock<BlobServiceClient> mockServiceClient = new();
        mockServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        AzureBlobStorageProvider sut = new(
            blobServiceClient: mockServiceClient.Object);

        bool result = await sut.BlobExistsAsync(It.IsAny<string>(), It.IsAny<string>());

        Assert.True(result);
    }

    [Fact]
    public async Task BlobExistsAsync_ReturnsFalse_WhenBlobDoesNotExist()
    {
        Mock<BlobClient> mockBlobClient = new();
        mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

        Mock<BlobContainerClient> mockContainerClient = new();
        mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        Mock<BlobServiceClient> mockServiceClient = new();
        mockServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        AzureBlobStorageProvider sut = new(
            blobServiceClient: mockServiceClient.Object);

        bool result = await sut.BlobExistsAsync(It.IsAny<string>(), It.IsAny<string>());

        Assert.False(result);
    }

    // Add false check

    [Fact]
    public async Task DownloadBlobAsStreamAsync_ReturnsStream_WhenBlobExists()
    {
        MemoryStream expectedStream = new(new byte[] { 1, 2, 3 });

        Mock<BlobClient> mockBlobClient = new();
        mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        mockBlobClient
            .Setup(x => x.DownloadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(
                BlobsModelFactory.BlobDownloadInfo(content: expectedStream),
                Mock.Of<Response>()));

        Mock<BlobContainerClient> mockContainerClient = new();
        mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        Mock<BlobServiceClient> mockServiceClient = new();
        mockServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        AzureBlobStorageProvider sut = new(mockServiceClient.Object);

        Stream result = await sut.DownloadBlobAsStreamAsync(It.IsAny<string>(), It.IsAny<string>());

        Assert.Equal(expectedStream, result);
    }

    [Fact]
    public async Task DownloadBlobAsStreamAsync_ThrowsFileNotFound_WhenBlobDoesNotExist()
    {
        Mock<BlobClient> mockBlobClient = new();
        mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

        Mock<BlobContainerClient> mockContainerClient = new();
        mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        Mock<BlobServiceClient> mockServiceClient = new();
        mockServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        AzureBlobStorageProvider sut = new(
            blobServiceClient: mockServiceClient.Object);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            sut.DownloadBlobAsStreamAsync(It.IsAny<string>(), It.IsAny<string>()));
    }


    // Helper to simulate async blob enumeration
    private static AsyncPageable<BlobItem> GetAsyncPageable(IEnumerable<BlobItem> items)
    {
        List<BlobItem> readOnlyItems = new(items);
        return AsyncPageable<BlobItem>.FromPages(new[] { Page<BlobItem>.FromValues(readOnlyItems, null, Mock.Of<Response>()) });
    }
}
