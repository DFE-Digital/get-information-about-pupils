namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
public sealed class ChainedCommandHandler<TIn> : IChainedCommandHandler<TIn>
{
    private ICommandHandler<TIn> _current;
    private ChainedCommandHandler<TIn>? _next;
    public ChainedCommandHandler(ICommandHandler<TIn> current)
    {
        ArgumentNullException.ThrowIfNull(current);
        _current = current;
        _next = null;
    }

    public bool CanHandle(TIn input) => _current.CanHandle(input);
    public void ChainNext(ICommandHandler<TIn> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        if(_next is null)
        {
            _next = new ChainedCommandHandler<TIn>(next);
            return;
        }

        // Else chain to the next handler
        _next.ChainNext(next);
    }
    public void Handle(TIn input)
    {
        // Invoke current if it can handle
        if (_current.CanHandle(input))
        {
            _current.Handle(input);
            return;
        }

        // Invoke next if it exists
        if(_next is not null)
        {
            _next.Handle(input);
            return;
        }
        
        // Explicit fail: nobody handled the input
        throw new InvalidOperationException($"No handler in the chain can handle input of type {typeof(TIn).Name}.");
    }
}
