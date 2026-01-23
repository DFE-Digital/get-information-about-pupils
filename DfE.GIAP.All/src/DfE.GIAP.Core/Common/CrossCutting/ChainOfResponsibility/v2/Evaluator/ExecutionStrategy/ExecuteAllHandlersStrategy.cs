using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Strategy;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.ExecutionStrategy;
internal class ExecuteAllHandlersStrategy<TIn> : IExecutionStrategy<TIn>
{
    public async ValueTask ExecuteAsync(TIn input, IHandlerChain<TIn, IEvaluationHandlerV2<TIn>> chain, CancellationToken token = default)
    {
        foreach (IEvaluationHandlerV2<TIn> handler in chain.Handlers)
        {
            await handler.HandleAsync(input, token);
            // Ignore whether the output fails or skips // TODO should this be configurable behaviour?
        }
    }
}

internal class ExecuteAllHandlersStrategy<TIn, TOut> : IExecutionStrategy<TIn, TOut>
{
    public async ValueTask<TOut> ExecuteAsync(TIn input, IHandlerChain<TIn, TOut, IEvaluationHandlerV2<TIn, TOut>> chain, CancellationToken token = default)
    {
        HandlerResult<TOut> output = default!;
        foreach (IEvaluationHandlerV2<TIn, TOut> handler in chain.Handlers)
        {
              output = await handler.HandleAsync(input, token);
            // Ignore whether the output fails
        }

        if(output.Result is null)
        {
            throw new ArgumentException("Final handler did not provide a response");
        }

        return output.Result;
    }
}
