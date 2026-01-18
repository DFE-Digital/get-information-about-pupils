using System.Collections;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
// TODO equality between chains?
public sealed class HandlerChain<T> : IHandlerChain<T>
{
    private readonly T[] _items;

    public HandlerChain(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items = items.ToArray();
        if (_items.Length == 0)
        {
            throw new ArgumentException("Handler chain cannot be empty");
        }
    }
    public T this[int index] => _items[index];

    public int Count => _items.Length;

    public IEnumerator<T> GetEnumerator() => _items.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
