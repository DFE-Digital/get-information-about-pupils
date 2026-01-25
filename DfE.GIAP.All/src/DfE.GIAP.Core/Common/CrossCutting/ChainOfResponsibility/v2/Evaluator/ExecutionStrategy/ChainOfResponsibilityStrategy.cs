using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Strategy;
public sealed class ChainOfResponsibilityStrategy<TIn> : IExecutionStrategy<TIn>
{
    public async ValueTask ExecuteAsync(
        TIn input,
        IHandlerChain<TIn, IEvaluationHandlerV2<TIn>> chain,
        CancellationToken ctx = default)
    {
        foreach (IEvaluationHandlerV2<TIn> item in chain.Handlers)
        {
            HandlerResult result = await item.HandleAsync(input, ctx);

            if (result.Status.Equals(HandlerResultStatus.Success))
            {
                return;
            }

            if (result.Status.Equals(HandlerResultStatus.Failed))
            {
                throw new ArgumentException("Attempt to handle failed", result.Exception);
            }
        }

        throw new InvalidOperationException("No handlers able to handle");
    }
}

public sealed class ChainOfResponsibilityStrategy<TIn, TOut> : IExecutionStrategy<TIn, TOut>
{
    public async ValueTask<TOut> ExecuteAsync(
        TIn input,
        IHandlerChain<TIn, TOut, IEvaluationHandlerV2<TIn, TOut>> chain,
        CancellationToken ctx = default)
    {
        foreach (IEvaluationHandlerV2<TIn, TOut> item in chain.Handlers)
        {
            HandlerResult<TOut> handlerResponse = await item.HandleAsync(input, ctx);

            if (handlerResponse.Status.Equals(HandlerResultStatus.Success))
            {
                return handlerResponse.Result!;
            }

            if (handlerResponse.Status.Equals(HandlerResultStatus.Failed))
            {
                throw new ArgumentException("Attempt to handle failed", handlerResponse.Exception);
            }
        }

        throw new InvalidOperationException("No handlers able to handle");
    }
}
