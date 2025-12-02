using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Glossary;

[Trait("Category", "Glossary Controller Unit Tests")]
public class GlossaryControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithPreparedFiles()
    {
        // Arrange
        Mock<IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>> mockGetPreparedFilesUseCase = new();
        Mock<IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>> mockDownloadPreparedFileUseCase = new();
        Mock<IEventLogger> mockEventLogger = new();

        List<BlobItemMetadata> blobItems = new List<BlobItemMetadata>
        {
            new BlobItemMetadata { Name = "File1.csv", LastModified = DateTimeOffset.UtcNow.AddDays(-1) },
            new BlobItemMetadata { Name = "File2.csv", LastModified = DateTimeOffset.UtcNow }
        };

        GetPreparedFilesResponse response = new GetPreparedFilesResponse(blobItems);

        mockGetPreparedFilesUseCase
            .Setup(x => x.HandleRequestAsync(It.IsAny<GetPreparedFilesRequest>()))
            .ReturnsAsync(response);

        GlossaryController controller = new(
            getPrePreparedFilesUseCase: mockGetPreparedFilesUseCase.Object,
            downloadPrePreparedFileUseCase: mockDownloadPreparedFileUseCase.Object,
            eventLogger: mockEventLogger.Object);

        // Act
        IActionResult result = await controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        GlossaryViewModel model = Assert.IsType<GlossaryViewModel>(viewResult.Model);
        Assert.Equal(2, model.PreparedMetadataFiles.Count);
        Assert.Equal("File2.csv", model.PreparedMetadataFiles.First().Name); // Ordered by date descending
    }

    [Fact]
    public async Task Index_OrdersPreparedFilesByDateDescending()
    {
        // Arrange
        Mock<IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>> mockGetPreparedFilesUseCase = new();
        Mock<IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>> mockDownloadPreparedFileUseCase = new();
        Mock<IEventLogger> mockEventLogger = new();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<BlobItemMetadata> blobItems = new()
        {
            new BlobItemMetadata { Name = "Oldest.csv", LastModified = now.AddDays(-2) },
            new BlobItemMetadata { Name = "Middle.csv", LastModified = now.AddDays(-1) },
            new BlobItemMetadata { Name = "Newest.csv", LastModified = now }
        };

        GetPreparedFilesResponse response = new(blobItems);

        mockGetPreparedFilesUseCase
            .Setup(x => x.HandleRequestAsync(It.IsAny<GetPreparedFilesRequest>()))
            .ReturnsAsync(response);

        GlossaryController controller = new(
            mockGetPreparedFilesUseCase.Object,
            mockDownloadPreparedFileUseCase.Object,
            mockEventLogger.Object);

        // Act
        IActionResult result = await controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        GlossaryViewModel model = Assert.IsType<GlossaryViewModel>(viewResult.Model);

        Assert.Equal(3, model.PreparedMetadataFiles.Count);
        Assert.Equal("Newest.csv", model.PreparedMetadataFiles[0].Name);
        Assert.Equal("Middle.csv", model.PreparedMetadataFiles[1].Name);
        Assert.Equal("Oldest.csv", model.PreparedMetadataFiles[2].Name);
    }

    [Fact]
    public async Task GetBulkUploadTemplateFile_ReturnsFileStreamResult()
    {
        // Arrange
        Mock<IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>> mockGetPreparedFilesUseCase = new();
        Mock<IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>> mockDownloadPreparedFileUseCase = new();
        Mock<IEventLogger> mockEventLogger = new();

        string fileName = "template.csv";
        string contentType = "text/csv";
        MemoryStream fileStream = new([1, 2, 3]);

        DownloadPreparedFileResponse response = new DownloadPreparedFileResponse(fileStream, fileName, contentType);

        mockDownloadPreparedFileUseCase
            .Setup(x => x.HandleRequestAsync(It.IsAny<DownloadPreparedFileRequest>()))
            .ReturnsAsync(response);

        GlossaryController controller = new(
            getPrePreparedFilesUseCase: mockGetPreparedFilesUseCase.Object,
            downloadPrePreparedFileUseCase: mockDownloadPreparedFileUseCase.Object,
            eventLogger: mockEventLogger.Object);

        // Act
        FileStreamResult result = await controller.GetBulkUploadTemplateFile(fileName);

        // Assert
        FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal(contentType, fileResult.ContentType);
        Assert.Equal(fileName, fileResult.FileDownloadName);
        Assert.Equal(fileStream, fileResult.FileStream);
    }
}
