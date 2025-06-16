using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;

public static class LoggerTestDoubles
{
    public static InMemoryLogger<T> MockLogger<T>()
    {
        InMemoryLogger<T> mockLogger = new();
        return mockLogger;
    }
}

public sealed class InMemoryLogger<T> : ILogger<T>
{
    public List<string> Logs { get; } = [];

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
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
