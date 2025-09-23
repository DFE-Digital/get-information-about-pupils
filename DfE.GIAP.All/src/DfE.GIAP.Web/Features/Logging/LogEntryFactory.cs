using DfE.GIAP.Core.Logging.Application;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public class LogEntryFactory : ILogEntryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LogEntryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public LogEntry<TPayload> CreateLogEntry<TPayload, TPayloadOptions>(TPayloadOptions options)
    {
        ILogPayloadBuilder<TPayload, TPayloadOptions> builder = _serviceProvider
            .GetRequiredService<ILogPayloadBuilder<TPayload, TPayloadOptions>>();

        TPayload payload = builder.Build(options);

        return new LogEntry<TPayload>
        {
            Timestamp = DateTime.UtcNow,
            Payload = payload
        };
    }
}
