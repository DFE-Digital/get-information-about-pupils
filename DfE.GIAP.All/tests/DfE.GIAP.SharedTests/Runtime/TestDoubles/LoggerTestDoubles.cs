using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.SharedTests.Runtime.TestDoubles;

// TODO: Remove once phased out ILogger in favor of ILoggerService
public static class LoggerTestDoubles
{
    public static InMemoryLogger<T> Fake<T>()
    {
        InMemoryLogger<T> mockLogger = new();
        return mockLogger;
    }
}

// TODO: Remove once phased out ILogger in favor of ILoggerService
public sealed class InMemoryLogger<T> : ILogger<T>
{
    public List<string> Logs { get; } = [];

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?,
        string> formatter)
    {
        Logs.Add(formatter(state, exception));
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }
}


public static class LoggerServiceTestDoubles
{
    public static InMemoryLoggerService MockLoggerService()
    {
        return new InMemoryLoggerService();
    }
}

public sealed class InMemoryLoggerService : IApplicationLoggerService
{
    public List<string> Logs { get; } = [];

    public void LogTrace(
        Core.Common.CrossCutting.Logging.Application.LogLevel level,
        string message,
        Exception? exception = null,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? context = null)
    {
        string logEntry = $"{message}";

        Logs.Add(logEntry);
    }
}
