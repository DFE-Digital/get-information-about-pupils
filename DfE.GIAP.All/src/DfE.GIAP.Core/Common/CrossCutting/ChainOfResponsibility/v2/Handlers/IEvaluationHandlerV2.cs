namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

internal interface IEvaluationHandlerV2<TIn>
{
    ValueTask<bool> CanHandleAsync(TIn input, CancellationToken ctx = default);
    ValueTask<HandlerResult> TryHandleAsync(TIn input, CancellationToken ctx = default);
}

internal interface IEvaluationHandlerV2<TIn, TOut>
{
    ValueTask<bool> CanHandleAsync(TIn input, CancellationToken ctx = default);
    ValueTask<HandlerResult<TOut>> TryHandleAsync(TIn input, CancellationToken ctx = default);
}
