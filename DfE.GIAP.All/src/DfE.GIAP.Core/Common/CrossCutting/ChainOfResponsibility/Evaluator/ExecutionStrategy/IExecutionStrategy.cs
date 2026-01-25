using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Chain;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.ExecutionStrategy;

// TODO should execution strategy always return an aggregated result object e.g. ValueTask<HandlerResults<TOut>>
public interface IExecutionStrategy<TIn>
{
    ValueTask ExecuteAsync(
        TIn input,
        IHandlerChain<TIn, IEvaluationHandler<TIn>> chain,
        CancellationToken token = default
    );
}
