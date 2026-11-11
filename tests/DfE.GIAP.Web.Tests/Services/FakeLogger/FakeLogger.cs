using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Web.Tests.Services.FakeLogger;

public abstract class FakeLogger<T> : ILogger<T>
{
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Log(logLevel, formatter(state, exception), exception);

    public abstract void Log(LogLevel logLevel, object state, Exception? exception = null);

    public virtual bool IsEnabled(LogLevel logLevel) => true;

    public abstract IDisposable BeginScope<TState>(TState state) where TState : notnull;
}
