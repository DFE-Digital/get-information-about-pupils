using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Chain;
public sealed class HandlerChainBuilder<TContext, THandler> : IHandlerChainBuilder<TContext, THandler>
    where THandler : IEvaluationHandler<TContext>
{
    private readonly List<THandler> _items;
    public HandlerChainBuilder()
    {
        _items = [];
    }

    public IHandlerChainBuilder<TContext, THandler> ChainNext(THandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _items.Add(handler);
        return this;
    }

    public IHandlerChain<TContext, THandler> Build() => new HandlerChain<TContext, THandler>(_items);
    public static HandlerChainBuilder<TContext, THandler> Create() => new();
    public static HandlerChainBuilder<TContext, THandler> Create(IEnumerable<THandler> handlers)
    {
        HandlerChainBuilder<TContext, THandler> builder = new();

        handlers?.ToList().ForEach(handler => builder.ChainNext(handler));

        return builder;
    }
}
