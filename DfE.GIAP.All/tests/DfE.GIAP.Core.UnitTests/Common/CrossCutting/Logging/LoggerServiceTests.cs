using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.UnitTests.Common.CrossCutting.Logging;

public class LoggerServiceTests
{
    private readonly Mock<ILogEntryFactory<TracePayloadOptions, TracePayload>> _factoryMock;

    public LoggerServiceTests()
    {
        _factoryMock = new Mock<ILogEntryFactory<TracePayloadOptions, TracePayload>>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_TraceLogHandlersNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LoggerService(null!, _factoryMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_When_TraceFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LoggerService(new List<ITraceLogHandler>(), null!));
    }

    [Fact]
    public void LogTrace_Calls_FactoryCreate()
    {
        // Arrange
        Mock<ITraceLogHandler> handlerMock = new();

        LoggerService sut = new(new[] { handlerMock.Object }, _factoryMock.Object);

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

        LoggerService sut = new(new[] { handlerMock.Object }, _factoryMock.Object);

        // Act
        sut.LogTrace(It.IsAny<LogLevel>(), It.IsAny<string>());

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<Log<TracePayload>>()), Times.Once);
    }
}
