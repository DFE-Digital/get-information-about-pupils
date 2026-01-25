using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Chain;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.ExecutionStrategy;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.Options;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator;

// TODO register hooks / Observer pattern for observation of TRequest or handlers allowing logging / other actions... EndOfChainBehaviour could be configured here too

// TODO could enrich context with e.g. TraceIdentifier pass from client

// TODO policies to guard whether execution should start before entering a handler chain - given a context..

// TODO IConverter<TIn, TOut> where the TOut of a given handlers execution, needs to feed into the next Tin for a handler....
// .... Could make the request muteable and register observer action ???
public sealed class Evaluator<TRequest> : IEvaluator<TRequest>
{
    private readonly IHandlerChain<
        TRequest,
            IEvaluationHandler<TRequest>> _handlerChain;

    public Evaluator(
        IHandlerChain<
            TRequest,
                IEvaluationHandler<TRequest>> handlerChain)
    {
        ArgumentNullException.ThrowIfNull(handlerChain);
        _handlerChain = handlerChain;
    }

    public async ValueTask EvaluateAsync(
        TRequest input, EvaluationOptions? options = null, CancellationToken ctx = default)
    {
        options ??= new();

        IExecutionStrategy<TRequest> strategy = new ChainOfResponsibilityStrategy<TRequest>();

        await strategy.ExecuteAsync(input, _handlerChain, ctx);
    }
}

