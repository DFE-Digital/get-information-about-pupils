using System.Collections.Concurrent;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Sinks;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace DfE.GIAP.Core.UnitTests.Common.CrossCutting.Logging.Application.Sinks;
public class AzureAppInsightTraceSinkTests
{
    private static (AzureAppInsightTraceSink sink, StubTelemetryChannel channel) CreateSink()
    {
        StubTelemetryChannel channel = new();
        TelemetryConfiguration config = new() { TelemetryChannel = channel };
        TelemetryClient client = new(config);
        AzureAppInsightTraceSink sink = new(client);
        return (sink, channel);
    }

    [Fact]
    public void Log_WithException_Calls_TrackException()
    {
        // Arrange
        (AzureAppInsightTraceSink sink, StubTelemetryChannel channel) = CreateSink();
        Exception ex = new("Exception");
        Log<TracePayload> log = LogFactoryTestDoubles.CreateDefaultTraceLog(exception: ex);

        // Act
        sink.Log(log);

        // Assert
        ITelemetry telemetry = Assert.Single(channel.SentTelemetries);
        ExceptionTelemetry exceptionTelemetry = Assert.IsType<ExceptionTelemetry>(telemetry);
        Assert.IsNotType<TraceTelemetry>(telemetry);
        Assert.Equal(ex, exceptionTelemetry.Exception);
    }

    [Fact]
    public void Log_WithoutException_CallsTrackTraceWithProperties()
    {
        // Arrange
        (AzureAppInsightTraceSink sink, StubTelemetryChannel channel) = CreateSink();
        Dictionary<string, object> context = new Dictionary<string, object> { { "Key1", "abc" }, { "Key2", "abc" } };

        Log<TracePayload> log = LogFactoryTestDoubles.CreateDefaultTraceLog(context: context);

        // Act
        sink.Log(log);

        // Assert
        ITelemetry telemetry = Assert.Single(channel.SentTelemetries);
        TraceTelemetry traceTelemetry = Assert.IsType<TraceTelemetry>(telemetry);

        Assert.Equal("abc", traceTelemetry.Properties["Key1"]);
        Assert.Equal("abc", traceTelemetry.Properties["Key2"]);
    }

    [Theory]
    [InlineData(LogLevel.Trace, SeverityLevel.Verbose)]
    [InlineData(LogLevel.Debug, SeverityLevel.Verbose)]
    [InlineData(LogLevel.Information, SeverityLevel.Information)]
    [InlineData(LogLevel.Warning, SeverityLevel.Warning)]
    [InlineData(LogLevel.Error, SeverityLevel.Error)]
    [InlineData(LogLevel.Critical, SeverityLevel.Critical)]
    [InlineData((LogLevel)999, SeverityLevel.Information)] // default case
    public void Log_MapsLogLevel_ToCorrectSeverity(LogLevel logLevel, SeverityLevel expectedSeverity)
    {
        // Arrange
        (AzureAppInsightTraceSink sink, StubTelemetryChannel channel) = CreateSink();
        Log<TracePayload> log = LogFactoryTestDoubles.CreateDefaultTraceLog(level: logLevel);

        // Act
        sink.Log(log);

        // Assert
        ITelemetry telemetry = Assert.Single(channel.SentTelemetries);
        TraceTelemetry traceTelemetry = Assert.IsType<TraceTelemetry>(telemetry);

        Assert.Equal(expectedSeverity, traceTelemetry.SeverityLevel);
    }
}


public class StubTelemetryChannel : ITelemetryChannel
{
    public ConcurrentBag<ITelemetry> SentTelemetries { get; } = new();

    public void Send(ITelemetry item) => SentTelemetries.Add(item);

    public void Flush() { }

    public bool? DeveloperMode { get; set; }
    public string? EndpointAddress { get; set; }
    public void Dispose() { }
}
