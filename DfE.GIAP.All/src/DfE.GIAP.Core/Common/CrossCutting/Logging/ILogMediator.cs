namespace DfE.GIAP.Core.Common.CrossCutting.Logging;
public interface ILogMediator
{
    void Publish(LogEntry entry);
}

public class LogMediator : ILogMediator
{
    private readonly IEnumerable<ILogEventHandler> _handlers;

    public LogMediator(IEnumerable<ILogEventHandler> handlers)
    {
        _handlers = handlers;
    }

    public void Publish(LogEntry logEntry)
    {
        foreach (ILogEventHandler handler in _handlers)
        {
            handler.Handle(logEntry);
        }
    }
}
