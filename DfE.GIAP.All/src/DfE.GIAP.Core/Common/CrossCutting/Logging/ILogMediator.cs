namespace DfE.GIAP.Core.Common.CrossCutting.Logging;
public interface ILogMediator
{
    void Publish(LogEntry entry);
}

public class LogMediator : ILogMediator
{
    private readonly IEnumerable<ILogRouter> _routers;

    public LogMediator(IEnumerable<ILogRouter> routers)
    {
        _routers = routers;
    }

    public void Publish(LogEntry entry)
    {
        foreach (ILogRouter router in _routers)
        {
            router.Route(entry);
        }
    }
}
