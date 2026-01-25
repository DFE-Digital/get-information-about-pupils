using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Chain;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.ExecutionStrategy;
public sealed class ChainOfResponsibilityStrategy<TIn> : IExecutionStrategy<TIn>
{
    public async ValueTask ExecuteAsync(
        TIn input,
        IHandlerChain<TIn, IEvaluationHandler<TIn>> chain,
        CancellationToken ctx = default)
    {
        foreach (IEvaluationHandler<TIn> item in chain.Handlers)
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
