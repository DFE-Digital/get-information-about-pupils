using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.PreparedDownloads.Application.UseCases.GetPreparedFiles;
public sealed class GetPreparedFilesUseCaseTests
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
            (useCase) => useCase.ListBlobsWithMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_BubblesException_WhenBlobStorageThrows()
    {
        // Arrange
        Mock<IBlobStorageProvider> mockBlobStorageProvider = BlobStorageProviderTestDoubles
            .MockForListBlobWithMetadata(() => throw new Exception("Blob error"));
        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(OrganisationScope.AllUsers);

        GetPreparedFilesUseCase useCase = new(
            blobStorageProvider: mockBlobStorageProvider.Object);
        GetPreparedFilesRequest request = new(pathContext);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await useCase.HandleRequestAsync(request);
        });
    }

    [Fact]
    public async Task HandleRequestAsync_Calls_BlobProvider_When_RequestIsValid()
    {
        // Arrange
        Mock<IBlobStorageProvider> mockBlobStorageProvider = BlobStorageProviderTestDoubles
            .MockForListBlobWithMetadata(Enumerable.Empty<BlobItemMetadata>);

        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(OrganisationScope.AllUsers);

        GetPreparedFilesUseCase sut = new(
            blobStorageProvider: mockBlobStorageProvider.Object);
        GetPreparedFilesRequest request = new(pathContext);

        // Act
        GetPreparedFilesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        mockBlobStorageProvider.Verify(
            (useCase) => useCase.ListBlobsWithMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once());

    }

    [Fact]
    public async Task HandleRequestAsync_Returns_ExpectedFiles()
    {
        // Arrange
        List<BlobItemMetadata> expectedFiles = new()
        {
            new()
            {
                Name = "file1.csv",
                ContentType = "text/csv",
            },
            new()
            {
                Name = "file2.pdf",
                ContentType = "application/pdf",
            }
        };

        Mock<IBlobStorageProvider> mockBlobStorageProvider = BlobStorageProviderTestDoubles
            .MockForListBlobWithMetadata(() => expectedFiles);

        BlobStoragePathContext pathContext = BlobStoragePathContext.Create(OrganisationScope.AllUsers);
        GetPreparedFilesRequest request = new(pathContext);
        GetPreparedFilesUseCase sut = new(
            blobStorageProvider: mockBlobStorageProvider.Object);

        // Act
        GetPreparedFilesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.Equal(expectedFiles, response.BlobStorageItems);
    }
}
