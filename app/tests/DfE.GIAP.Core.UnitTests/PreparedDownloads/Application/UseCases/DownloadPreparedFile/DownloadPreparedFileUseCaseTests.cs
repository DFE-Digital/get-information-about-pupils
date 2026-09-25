using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.PreparedDownloads.Application.UseCases.DownloadPreparedFile;

public sealed class DownloadPreparedFileUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullBlobStorageProvider()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            DownloadPreparedFileUseCase useCase = new(
                blobStorageProvider: null!);
        });
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        Mock<IBlobStorageProvider> mockBlobStorageProvider = BlobStorageProviderTestDoubles.Default();
        DownloadPreparedFileUseCase sut = new(
            blobStorageProvider: mockBlobStorageProvider.Object);

        Func<Task> act = () => sut.HandleRequestAsync(request: null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockBlobStorageProvider.Verify(
            (useCase) => useCase.DownloadBlobAsStreamAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never());
    }


    [Fact]
    public async Task HandleRequestAsync_Calls_BlobProvider_When_RequestIsValid()
    {
        // Arrange
        Mock<IBlobStorageProvider> mockBlobStorageProvider = BlobStorageProviderTestDoubles.Default();
        MemoryStream expectedStream = new(new byte[] { 1, 2, 3 });
        string fileName = "test.csv";
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(OrganisationScope.AllUsers);

        mockBlobStorageProvider
            .Setup(x => x.DownloadBlobAsStreamAsync("giapdownloads", "AllUsers/Metadata/" + fileName, default))
            .ReturnsAsync(expectedStream);

        DownloadPreparedFileUseCase sut = new(
            blobStorageProvider: mockBlobStorageProvider.Object);
        DownloadPreparedFileRequest request = new(fileName, pathContext);

        // Act
        DownloadPreparedFileResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.Equal(fileName, response.FileName);
        Assert.Equal(expectedStream, response.FileStream);
        mockBlobStorageProvider
            .Verify(x => x.DownloadBlobAsStreamAsync("giapdownloads", "AllUsers/Metadata/" + fileName, default), Times.Once());
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_WhenBlobStorageThrows()
    {
        // Arrange
        Mock<IBlobStorageProvider> mockBlobStorageProvider = BlobStorageProviderTestDoubles
            .MockForDownloadBlobAsStream(() => throw new Exception("Blob error"));
        string fileName = "missing.csv";
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(OrganisationScope.AllUsers);

        DownloadPreparedFileUseCase useCase = new(
            blobStorageProvider: mockBlobStorageProvider.Object);
        DownloadPreparedFileRequest request = new(fileName, pathContext);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await useCase.HandleRequestAsync(request);
        });
    }
}
