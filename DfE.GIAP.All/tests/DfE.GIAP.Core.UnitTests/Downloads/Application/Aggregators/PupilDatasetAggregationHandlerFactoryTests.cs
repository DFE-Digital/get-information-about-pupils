using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;


namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators;


public sealed class PupilDatasetAggregationHandlerFactoryTests
{

    [Fact]
    public void Constructor_StoresHandlersBySupportedDownloadType()
    {
        // Arrange
        Mock<IPupilDatasetAggregationHandler> handlerMock = new();
        handlerMock.Setup(h => h.SupportedDownloadType).Returns(PupilDownloadType.FurtherEducation);

        // Act
        PupilDatasetAggregationHandlerFactory sut = new(new[] { handlerMock.Object });

        // Assert
        Task<PupilDatasetCollection> resultTask = sut.AggregateAsync(
            PupilDownloadType.FurtherEducation,
            Enumerable.Empty<string>(),
            Enumerable.Empty<Dataset>());

        Assert.NotNull(resultTask);
    }

    [Fact]
    public void Constructor_Throws_WhenDuplicateSupportedDownloadTypesProvided()
    {
        // Arrange
        Mock<IPupilDatasetAggregationHandler> handler1 = new();
        handler1.Setup(h => h.SupportedDownloadType).Returns(PupilDownloadType.FurtherEducation);

        Mock<IPupilDatasetAggregationHandler> handler2 = new();
        handler2.Setup(h => h.SupportedDownloadType).Returns(PupilDownloadType.FurtherEducation);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PupilDatasetAggregationHandlerFactory(new[] { handler1.Object, handler2.Object }));
    }


    [Fact]
    public async Task AggregateAsync_InvokesCorrectHandler_ForMatchingDownloadType()
    {
        // Arrange
        string[] pupilIds = new[] { "A", "B" };
        Dataset[] datasets = new[] { Dataset.FE_PP };

        PupilDatasetCollection expectedResult = new();

        Mock<IPupilDatasetAggregationHandler> handlerMock = new();
        handlerMock.Setup(h => h.SupportedDownloadType)
            .Returns(PupilDownloadType.FurtherEducation);
        handlerMock.Setup(h => h.AggregateAsync(pupilIds, datasets, default))
            .ReturnsAsync(expectedResult);

        PupilDatasetAggregationHandlerFactory sut = new(new[] { handlerMock.Object });

        // Act
        PupilDatasetCollection result = await sut.AggregateAsync(PupilDownloadType.FurtherEducation, pupilIds, datasets);

        // Assert
        Assert.Same(expectedResult, result);
        handlerMock.Verify(h => h.AggregateAsync(pupilIds, datasets, default), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_PassesCorrectArgumentsToHandler()
    {
        // Arrange
        string[] pupilIds = new[] { "X", "Y" };
        Dataset[] datasets = new[] { Dataset.SEN };

        Mock<IPupilDatasetAggregationHandler> handlerMock = new Mock<IPupilDatasetAggregationHandler>();
        handlerMock.Setup(h => h.SupportedDownloadType).Returns(PupilDownloadType.FurtherEducation);
        handlerMock.Setup(h => h.AggregateAsync(It.IsAny<IEnumerable<string>>(),
                                                It.IsAny<IEnumerable<Dataset>>(),
                                                default))
                   .ReturnsAsync(new PupilDatasetCollection());

        PupilDatasetAggregationHandlerFactory sut = new(new[] { handlerMock.Object });

        // Act
        await sut.AggregateAsync(PupilDownloadType.FurtherEducation, pupilIds, datasets);

        // Assert
        handlerMock.Verify(h => h.AggregateAsync(
            It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(pupilIds)),
            It.Is<IEnumerable<Dataset>>(ds => ds.SequenceEqual(datasets)),
            default),
            Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_Throws_WhenNoHandlerRegisteredForDownloadType()
    {
        // Arrange
        Mock<IPupilDatasetAggregationHandler> handlerMock = new();
        handlerMock.Setup(h => h.SupportedDownloadType)
            .Returns(PupilDownloadType.FurtherEducation);

        PupilDatasetAggregationHandlerFactory factory = new(new[] { handlerMock.Object });

        // Act
        NotSupportedException ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            factory.AggregateAsync(PupilDownloadType.PupilPremium, Enumerable.Empty<string>(), Enumerable.Empty<Dataset>()));

        // Assert
        Assert.Contains("No pupil aggregator handler registered", ex.Message);
    }
}
