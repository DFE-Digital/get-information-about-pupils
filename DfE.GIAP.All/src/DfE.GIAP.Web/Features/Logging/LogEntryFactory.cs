using DfE.GIAP.Core.Logging.Application;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public class LogEntryFactory : ILogEntryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LogEntryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public LogEntry<TPayload> CreateLogEntry<TPayload>(ILogPayloadOptions options)
    {
        ILogPayloadBuilder<TPayload> builder = _serviceProvider.GetRequiredService<ILogPayloadBuilder<TPayload>>();
        TPayload payload = builder.BuildPayload(options);

        return new LogEntry<TPayload>
        {
            Timestamp = DateTime.UtcNow,
            Payload = payload
        };
    }
}
