using System.Collections;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
// TODO equality between chains?
public sealed class HandlerChain<TContext, THandler> : IHandlerChain<TContext, THandler>
    where THandler : IEvaluationHandlerV2<TContext>
{
    public HandlerChain(IEnumerable<THandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        if (!handlers.Any())
        {
            throw new ArgumentException("No handlers registered");
        }

        Handlers = handlers.ToList().AsReadOnly();
    }

    public IReadOnlyList<THandler> Handlers { get; }
}


public sealed class HandlerChain<TContext, TOutput, THandler> : IHandlerChain<TContext, TOutput, THandler>
    where THandler : IEvaluationHandlerV2<TContext, TOutput>
{
    public HandlerChain(IEnumerable<THandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        if (!handlers.Any())
        {
            throw new ArgumentException("No handlers registered");
        }

        Handlers = handlers.ToList().AsReadOnly();
    }

    public IReadOnlyList<THandler> Handlers { get; }
}
