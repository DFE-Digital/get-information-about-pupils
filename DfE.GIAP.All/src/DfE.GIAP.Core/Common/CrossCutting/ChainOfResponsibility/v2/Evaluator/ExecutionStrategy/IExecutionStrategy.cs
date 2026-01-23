using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Strategy;

// TODO should execution strategy always return an aggregated result object e.g. ValueTask<HandlerResults<TOut>>
public interface IExecutionStrategy<TIn>
{
    ValueTask ExecuteAsync(
        TIn input,
        IHandlerChain<TIn, IEvaluationHandlerV2<TIn>> chain,
        CancellationToken token = default
    );
}

public interface IExecutionStrategy<TIn, TOut>
{
    ValueTask<TOut> ExecuteAsync(
        TIn input,
        IHandlerChain<TIn, TOut, IEvaluationHandlerV2<TIn, TOut>> chain,
        CancellationToken token = default
    );
}
