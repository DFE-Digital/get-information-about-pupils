using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.UnitTests.Common.CrossCutting.Logging.Application;

public class ApplicationLoggerServiceTests
{
    private readonly Mock<IApplicationLogEntryFactory<TracePayloadOptions, TracePayload>> _factoryMock;

    public ApplicationLoggerServiceTests()
    {
        _factoryMock = new Mock<IApplicationLogEntryFactory<TracePayloadOptions, TracePayload>>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_TraceLogHandlersNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ApplicationLoggerService(null!, _factoryMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_TraceFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ApplicationLoggerService(new List<ITraceLogHandler>(), null!));
    }

    [Fact]
    public void LogTrace_Calls_FactoryCreate()
    {
        // Arrange
        Mock<ITraceLogHandler> handlerMock = new();

        ApplicationLoggerService sut = new(new[] { handlerMock.Object }, _factoryMock.Object);

        // Act
        sut.LogTrace(
            It.IsAny<LogLevel>(),
            It.IsAny<string>());

        // Assert
        _factoryMock.Verify(f => f.Create(It.IsAny<TracePayloadOptions>()), Times.Once);
    }

    [Fact]
    public void LogTrace_Calls_TraceHandler()
    {
        // Arrange
        Mock<ITraceLogHandler> handlerMock = new();

        _factoryMock
            .Setup(f => f.Create(It.IsAny<TracePayloadOptions>()))
            .Returns(It.IsAny<Log<TracePayload>>());

        ApplicationLoggerService sut = new(new[] { handlerMock.Object }, _factoryMock.Object);

        // Act
        sut.LogTrace(It.IsAny<LogLevel>(), It.IsAny<string>());

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<Log<TracePayload>>()), Times.Once);
    }
}
