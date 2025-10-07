using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Configuration;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.Common.CrossCutting.Logging.Handlers;
public class TraceLogHandlerTests
{
    private static IOptions<LoggingOptions> CreateOptions(Dictionary<string, SinkConfig> sinks)
    {
        return Options.Create(new LoggingOptions
        {
            Trace = new TraceLogConfig { Sinks = sinks }
        });
    }

    [Fact]
    public void Handle_CallsSink_WhenLevelAccepted()
    {
        // Arrange
        Mock<ITraceLogSink> sinkMock = new();
        sinkMock.SetupGet(s => s.Name).Returns("TestSink");

        Dictionary<string, SinkConfig> configs = new()
        {
            ["TestSink"] = new SinkConfig
            {
                Enabled = true,
                AcceptedLogLevels = new List<LogLevel> { LogLevel.Warning }
            }
        };

        IOptions<LoggingOptions> options = CreateOptions(configs);
        TraceLogHandler handler = new(options, new[] { sinkMock.Object });

        Log<TracePayload> logEntry = LogFactoryTestDoubles
            .CreateDefaultTraceLog(level: LogLevel.Warning);

        // Act
        handler.Handle(logEntry);

        // Assert
        sinkMock.Verify(s => s.Log(logEntry), Times.Once);
    }

    [Fact]
    public void Handle_DoesNotCallSink_WhenLevelNotAccepted()
    {
        // Arrange
        Mock<ITraceLogSink> sinkMock = new();
        sinkMock.SetupGet(s => s.Name).Returns("TestSink");

        Dictionary<string, SinkConfig> configs = new()
        {
            ["TestSink"] = new SinkConfig
            {
                Enabled = true,
                AcceptedLogLevels = new List<LogLevel> { LogLevel.Error } // only Error accepted
            }
        };

        IOptions<LoggingOptions> options = CreateOptions(configs);
        TraceLogHandler handler = new(options, new[] { sinkMock.Object });

        Log<TracePayload> logEntry = LogFactoryTestDoubles.CreateDefaultTraceLog(level: LogLevel.Information);

        // Act
        handler.Handle(logEntry);

        // Assert
        sinkMock.Verify(s => s.Log(It.IsAny<Log<TracePayload>>()), Times.Never);
    }

    [Fact]
    public void Handle_DoesNotCallSink_When_SinkDisabled()
    {
        // Arrange
        Mock<ITraceLogSink> sinkMock = new();
        sinkMock.SetupGet(s => s.Name).Returns("TestSink");

        Dictionary<string, SinkConfig> configs = new Dictionary<string, SinkConfig>
        {
            ["TestSink"] = new SinkConfig
            {
                Enabled = false,
                AcceptedLogLevels = new List<LogLevel> { LogLevel.Warning }
            }
        };

        IOptions<LoggingOptions> options = CreateOptions(configs);
        TraceLogHandler handler = new TraceLogHandler(options, new[] { sinkMock.Object });

        Log<TracePayload> logEntry = LogFactoryTestDoubles.CreateDefaultTraceLog(level: LogLevel.Warning);

        // Act
        handler.Handle(logEntry);

        // Assert
        sinkMock.Verify(s => s.Log(It.IsAny<Log<TracePayload>>()), Times.Never);
    }

    [Fact]
    public void Handle_IgnoresSinks_When_NotInConfig()
    {
        // Arrange
        Mock<ITraceLogSink> sinkMock = new();
        sinkMock.SetupGet(s => s.Name).Returns("UnconfiguredSink");

        Dictionary<string, SinkConfig> configs = new Dictionary<string, SinkConfig>(); // no entry for this sink
        IOptions<LoggingOptions> options = CreateOptions(configs);

        TraceLogHandler handler = new(options, new[] { sinkMock.Object });
        Log<TracePayload> logEntry = LogFactoryTestDoubles.CreateDefaultTraceLog(level: LogLevel.Warning);

        // Act
        handler.Handle(logEntry);

        // Assert
        sinkMock.Verify(s => s.Log(It.IsAny<Log<TracePayload>>()), Times.Never);
    }

    [Fact]
    public void Handle_CallsAllEnabledSinks_WhenMultipleConfigured()
    {
        // Arrange
        Mock<ITraceLogSink> sink1 = new Mock<ITraceLogSink>();
        Mock<ITraceLogSink> sink2 = new Mock<ITraceLogSink>();
        sink1.SetupGet(s => s.Name).Returns("Sink1");
        sink2.SetupGet(s => s.Name).Returns("Sink2");

        Dictionary<string, SinkConfig> configs = new Dictionary<string, SinkConfig>
        {
            ["Sink1"] = new SinkConfig
            {
                Enabled = true,
                AcceptedLogLevels = new List<LogLevel> { LogLevel.Information }
            },
            ["Sink2"] = new SinkConfig
            {
                Enabled = true,
                AcceptedLogLevels = new List<LogLevel> { LogLevel.Information }
            }
        };

        IOptions<LoggingOptions> options = CreateOptions(configs);
        TraceLogHandler handler = new(options, new[] { sink1.Object, sink2.Object });

        Log<TracePayload> logEntry = LogFactoryTestDoubles.CreateDefaultTraceLog(level: LogLevel.Information);

        // Act
        handler.Handle(logEntry);

        // Assert
        sink1.Verify(s => s.Log(logEntry), Times.Once);
        sink2.Verify(s => s.Log(logEntry), Times.Once);
    }
}
