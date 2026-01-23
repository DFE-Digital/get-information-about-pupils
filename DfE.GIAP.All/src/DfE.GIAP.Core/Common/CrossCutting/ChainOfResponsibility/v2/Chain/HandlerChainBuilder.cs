using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
public sealed class HandlerChainBuilder<TContext, THandler> : IHandlerChainBuilder<TContext, THandler>
    where THandler : IEvaluationHandlerV2<TContext>
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


public sealed class HandlerChainBuilder<TContext, TOutput, THandler> : IHandlerChainBuilder<TContext, TOutput, THandler>
    where THandler : IEvaluationHandlerV2<TContext, TOutput>
{
    private readonly List<THandler> _items;
    public HandlerChainBuilder()
    {
        _items = [];
    }

    public IHandlerChainBuilder<TContext, TOutput, THandler> ChainNext(THandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _items.Add(handler);
        return this;
    }

    public IHandlerChain<TContext, TOutput, THandler> Build() => new HandlerChain<TContext, TOutput, THandler>(_items);
    public static HandlerChainBuilder<TContext, TOutput, THandler> Create() => new();
    public static HandlerChainBuilder<TContext, TOutput, THandler> Create(IEnumerable<THandler> handlers)
    {
        HandlerChainBuilder<TContext, TOutput, THandler> builder = new();

        handlers?.ToList().ForEach(handler => builder.ChainNext(handler));

        return builder;
    }
}
