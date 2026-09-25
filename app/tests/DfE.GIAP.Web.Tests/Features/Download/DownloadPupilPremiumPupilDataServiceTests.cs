using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Web.Features.Downloads.Services;
using Microsoft.AspNetCore.Mvc;
using DownloadOperationType = DfE.GIAP.Core.Common.CrossCutting.Logging.Events.DownloadOperationType;

namespace DfE.GIAP.Web.Tests.Features.Download;

public sealed class DownloadPupilPremiumPupilDataServiceTests
{
    [Fact]
    public void Constructor_Throws_When_DownloadDataUseCase_Is_Null()
    {
        // Arrange
        Mock<IEventLogger> eventLogger = new();

        // Act
        Func<DownloadPupilPremiumPupilDataService> construct =
            () => new DownloadPupilPremiumPupilDataService(
                    downloadDataUseCase: null!,
                    eventLogger: eventLogger.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_EventLogger_Is_Null()
    {
        // Arrange
        Mock<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>> useCase = new();

        // Act
        Func<DownloadPupilPremiumPupilDataService> construct =
            () => new DownloadPupilPremiumPupilDataService(
                    downloadDataUseCase: useCase.Object,
                    eventLogger: null!);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task DownloadAsync_Calls_UseCase_With_Filtered_Upns_And_Expected_RequestFields()
    {
        // Arrange
        List<string?> inputUpns =
            [
                "UPN-1",
                null!,
                string.Empty,
                "  ",
                "\n",
                "UPN-2"
            ];

        DownloadOperationType downloadEventType = DownloadOperationType.Search;

        DownloadPupilDataResponse useCaseResponse =
            new(FileContents: [0x01, 0x02], FileName: "pupil-premium.csv", ContentType: "text/csv");

        Mock<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>> useCaseMock = new();
        useCaseMock
            .Setup(useCase => useCase.HandleRequestAsync(It.IsAny<DownloadPupilDataRequest>()))
            .ReturnsAsync(useCaseResponse);

        Mock<IEventLogger> eventLogger = new();

        DownloadPupilPremiumPupilDataService sut = new(useCaseMock.Object, eventLogger.Object);

        // Act
        DownloadPupilPremiumFilesResponse result =
            await sut.DownloadAsync(inputUpns, downloadEventType, CancellationToken.None);

        // Assert

        List<string> expectedUpns = ["UPN-1", "UPN-2"];

        useCaseMock.Verify(u => u.HandleRequestAsync(
            It.Is<DownloadPupilDataRequest>(req =>
                req.SelectedPupils.SequenceEqual(expectedUpns) &&
                req.SelectedDatasets.Count() == 1 &&
                req.SelectedDatasets.Contains(Core.Downloads.Application.Enums.Dataset.PP) &&
                req.DownloadType == Core.Downloads.Application.Enums.PupilDownloadType.PupilPremium &&
                req.FileFormat == FileFormat.Csv
            )), Times.Once);

        eventLogger.Verify(logger => logger.LogDownload(
            downloadEventType,
            DownloadFileFormat.CSV,
            DownloadEventType.PP,
            null,
            null), Times.Once);

        Assert.NotNull(result);
        Assert.True(result.HasData);
        IActionResult actionResult = result.GetResult();
        Assert.IsType<FileStreamResult>(actionResult);
    }
}
