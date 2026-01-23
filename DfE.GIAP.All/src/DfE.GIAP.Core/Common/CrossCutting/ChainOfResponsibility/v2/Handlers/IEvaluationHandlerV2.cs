namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

public interface IEvaluationHandlerV2<TIn>
{
    ValueTask<HandlerResult> HandleAsync(TIn input, CancellationToken ctx = default);
}

public interface IEvaluationHandlerV2<TIn, TOut>
{
    ValueTask<HandlerResult<TOut>> HandleAsync(TIn input, CancellationToken ctx = default);
}
