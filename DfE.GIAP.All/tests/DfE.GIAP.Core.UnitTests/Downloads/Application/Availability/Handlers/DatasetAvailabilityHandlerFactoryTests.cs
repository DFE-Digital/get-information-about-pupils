using DfE.GIAP.Core.Downloads.Application.Availability;
using DfE.GIAP.Core.Downloads.Application.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Availability.Handlers;

public sealed class DatasetAvailabilityHandlerFactoryTests
{
    [Fact]
    public void GetDatasetAvailabilityHandler_ReturnsCorrectHandler_WhenTypeIsRegistered()
    {
        // Arrange
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(PupilDownloadType.FurtherEducation);
        DatasetAvailabilityHandlerFactory factory = new(new[] { handler });

        // Act
        IDatasetAvailabilityHandler result = factory.GetDatasetAvailabilityHandler(PupilDownloadType.FurtherEducation);

        // Assert
        Assert.Equal(PupilDownloadType.FurtherEducation, result.SupportedDownloadType);
    }

    [Fact]
    public void GetDatasetAvailabilityHandler_Throws_NotSupportedException_WhenTypeIsNotRegistered()
    {
        // Arrange
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(PupilDownloadType.FurtherEducation);
        DatasetAvailabilityHandlerFactory factory = new DatasetAvailabilityHandlerFactory(new[] { handler });

        // Act & Assert
        NotSupportedException ex = Assert.Throws<NotSupportedException>(() =>
            factory.GetDatasetAvailabilityHandler(PupilDownloadType.NPD));

        Assert.Contains($"No dataset availability handler registered for DownloadType '{PupilDownloadType.NPD}'", ex.Message);
    }

    [Fact]
    public void Constructor_Registers_MultipleHandlersCorrectly()
    {
        // Arrange
        IDatasetAvailabilityHandler feHandler = DatasetAvailabilityHandlerTestDouble.Create(PupilDownloadType.FurtherEducation);
        IDatasetAvailabilityHandler npdHandler = DatasetAvailabilityHandlerTestDouble.Create(PupilDownloadType.NPD);

        DatasetAvailabilityHandlerFactory factory = new(new[] { feHandler, npdHandler });

        // Act
        IDatasetAvailabilityHandler feResult = factory.GetDatasetAvailabilityHandler(PupilDownloadType.FurtherEducation);
        IDatasetAvailabilityHandler npdResult = factory.GetDatasetAvailabilityHandler(PupilDownloadType.NPD);

        // Assert
        Assert.Equal(PupilDownloadType.FurtherEducation, feResult.SupportedDownloadType);
        Assert.Equal(PupilDownloadType.NPD, npdResult.SupportedDownloadType);
    }
}
