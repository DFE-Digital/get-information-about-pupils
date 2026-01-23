using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.ExecutionStrategy;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Options;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Strategy;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator;

// TODO register hooks / Observer pattern for observation of TRequest or handlers allowing logging / other actions... EndOfChainBehaviour could be configured here too

// TODO could enrich context with e.g. TraceIdentifier pass from client

// TODO policies to guard whether execution should start before entering a handler chain - given a context..

// TODO IConverter<TIn, TOut> where the TOut of a given handlers execution, needs to feed into the next Tin for a handler....
// .... Could make the request muteable and register observer action ???
public sealed class EvaluatorV2<TRequest> : IEvaluatorV2<TRequest>
{
    private readonly IHandlerChain<
        TRequest,
            IEvaluationHandlerV2<TRequest>> _handlerChain;

    public EvaluatorV2(
        IHandlerChain<
            TRequest,
                IEvaluationHandlerV2<TRequest>> handlerChain)
    {
        ArgumentNullException.ThrowIfNull(handlerChain);
        _handlerChain = handlerChain;
    }

    public async ValueTask EvaluateAsync(
        TRequest input, EvaluationOptions? options = null, CancellationToken ctx = default)
    {
        options ??= new();
        IExecutionStrategy<TRequest> strategy = options.Mode == ChainExecutionMode.ChainOfResponsibility
            ? new ChainOfResponsibilityStrategy<TRequest>()
            : new ExecuteAllHandlersStrategy<TRequest>();

        await strategy.ExecuteAsync(input, _handlerChain, ctx);
    }
}

public sealed class EvaluatorV2<TRequest, TResponse> : IEvaluatorV2<TRequest, TResponse>
{
    private readonly IHandlerChain<
        TRequest,
            TResponse,
                IEvaluationHandlerV2<TRequest, TResponse>> _handlerChain;

    public EvaluatorV2(
        IHandlerChain<
            TRequest,
                TResponse,
                    IEvaluationHandlerV2<TRequest, TResponse>> handlerChain)
    {
        ArgumentNullException.ThrowIfNull(handlerChain);
        _handlerChain = handlerChain;
    }

    public async ValueTask<TResponse> EvaluateAsync(
        TRequest input, EvaluationOptions? options = null, CancellationToken ctx = default)
    {
        options ??= new();

        IExecutionStrategy<TRequest, TResponse> strategy = options.Mode == ChainExecutionMode.ChainOfResponsibility
            ? new ChainOfResponsibilityStrategy<TRequest, TResponse>()
            : new ExecuteAllHandlersStrategy<TRequest, TResponse>();

        return await strategy.ExecuteAsync(input, _handlerChain, ctx);
    }
}
