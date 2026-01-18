namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
public sealed class HandlerChainBuilder<T> : IHandlerChainBuilder<T>
{
    private readonly List<T> _items;
    public HandlerChainBuilder()
    {
        _items = [];
    }

    public IHandlerChainBuilder<T> ChainNext(T handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _items.Add(handler);
        return this;
    }
    public IHandlerChain<T> Build() => new HandlerChain<T>(_items);
    public static HandlerChainBuilder<T> Create() => new();
    public static HandlerChainBuilder<T> Create(IEnumerable<T> handlers)
    {
        HandlerChainBuilder<T> builder = new();

        handlers?.ToList().ForEach(handler => builder.ChainNext(handler));

        return builder;
    }
}
