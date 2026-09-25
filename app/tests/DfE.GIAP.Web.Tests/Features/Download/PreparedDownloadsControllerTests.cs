using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
using DfE.GIAP.Web.Features.Downloads.Controllers;
using DfE.GIAP.Web.Tests.Shared.HttpContext;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.PreparedDownload;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Tests.Features.Download;

[Trait("PreparedDownloads", "PreparedDownloads Controller Unit Tests")]
public class PreparedDownloadsControllerTests
{
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

        PreparedDownloadsController controller = new(
            mockGetPreparedFilesUseCase.Object,
            mockDownloadPreparedFileUseCase.Object,
            mockEventLogger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = HttpContextTestDoubles.WithUser(UserClaimsPrincipalFake.GetLAUserClaimsPrincipal())
        };

        // Act
        IActionResult result = await controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        PreparedDownloadsViewModel model = Assert.IsType<PreparedDownloadsViewModel>(viewResult.Model);

        Assert.Equal(3, model.PreparedDownloadFiles.Count);
        Assert.Equal("Newest.csv", model.PreparedDownloadFiles[0].Name);
        Assert.Equal("Middle.csv", model.PreparedDownloadFiles[1].Name);
        Assert.Equal("Oldest.csv", model.PreparedDownloadFiles[2].Name);
    }

    [Fact]
    public async Task DownloadPrePreparedFile_ReturnsFileStreamResult()
    {
        // Arrange
        Mock<IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>> mockGetPreparedFilesUseCase = new();
        Mock<IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>> mockDownloadPreparedFileUseCase = new();
        Mock<IEventLogger> mockEventLogger = new();

        string fileName = "template.csv";
        string contentType = "text/csv";
        MemoryStream fileStream = new([1, 2, 3]);

        DownloadPreparedFileResponse response = new(fileStream, fileName, contentType);

        mockDownloadPreparedFileUseCase
            .Setup(x => x.HandleRequestAsync(It.IsAny<DownloadPreparedFileRequest>()))
            .ReturnsAsync(response);

        PreparedDownloadsController controller = new(
            mockGetPreparedFilesUseCase.Object,
            mockDownloadPreparedFileUseCase.Object,
            mockEventLogger.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = HttpContextTestDoubles.WithUser(UserClaimsPrincipalFake.GetLAUserClaimsPrincipal())
        };

        // Act
        FileStreamResult result = await controller.DownloadPrePreparedFile(fileName);

        // Assert
        Assert.Equal(contentType, result.ContentType);
        Assert.Equal(fileName, result.FileDownloadName);
        Assert.Equal(fileStream, result.FileStream);
    }
}
