using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Availability.Handlers;

public sealed class DatasetAvailabilityHandlerFactoryTests
{
    [Fact]
    public void GetDatasetAvailabilityHandler_ReturnsCorrectHandler_WhenTypeIsRegistered()
    {
        // Arrange
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(DownloadType.FurtherEducation);
        DatasetAvailabilityHandlerFactory factory = new(new[] { handler });

        // Act
        IDatasetAvailabilityHandler result = factory.GetDatasetAvailabilityHandler(DownloadType.FurtherEducation);

        // Assert
        Assert.Equal(DownloadType.FurtherEducation, result.SupportedDownloadType);
    }

    [Fact]
    public void GetDatasetAvailabilityHandler_Throws_NotSupportedException_WhenTypeIsNotRegistered()
    {
        // Arrange
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(DownloadType.FurtherEducation);
        DatasetAvailabilityHandlerFactory factory = new DatasetAvailabilityHandlerFactory(new[] { handler });

        // Act & Assert
        NotSupportedException ex = Assert.Throws<NotSupportedException>(() =>
            factory.GetDatasetAvailabilityHandler(DownloadType.NPD));

        Assert.Contains($"No dataset availability handler registered for DownloadType '{DownloadType.NPD}'", ex.Message);
    }

    [Fact]
    public void Constructor_Registers_MultipleHandlersCorrectly()
    {
        // Arrange
        IDatasetAvailabilityHandler feHandler = DatasetAvailabilityHandlerTestDouble.Create(DownloadType.FurtherEducation);
        IDatasetAvailabilityHandler npdHandler = DatasetAvailabilityHandlerTestDouble.Create(DownloadType.NPD);

        DatasetAvailabilityHandlerFactory factory = new(new[] { feHandler, npdHandler });

        // Act
        IDatasetAvailabilityHandler feResult = factory.GetDatasetAvailabilityHandler(DownloadType.FurtherEducation);
        IDatasetAvailabilityHandler npdResult = factory.GetDatasetAvailabilityHandler(DownloadType.NPD);

        // Assert
        Assert.Equal(DownloadType.FurtherEducation, feResult.SupportedDownloadType);
        Assert.Equal(DownloadType.NPD, npdResult.SupportedDownloadType);
    }
}
