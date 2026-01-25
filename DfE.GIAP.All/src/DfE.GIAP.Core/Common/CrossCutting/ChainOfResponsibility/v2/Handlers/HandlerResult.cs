namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

// TODO consider Elapsed..
// TODO consider Metadata Dictionary<string, object?>
public record HandlerResult
{
    public HandlerResult(HandlerResultStatus status, Exception? exception = null)
    {
        Status = status;
        Exception = exception;
    }

    public HandlerResultStatus Status { get; }
    public Exception? Exception { get; }
}

public record HandlerResult<TOut> : HandlerResult
{
    public HandlerResult(
        HandlerResultStatus status,
        Exception? exception = null) : base(status, exception)
    {

    }

    public HandlerResult(
        HandlerResultStatus status,
        TOut? result,
        Exception? exception = null) : this(status, exception)
    {
        Result = result;
    }

    public TOut? Result { get; }
}


public static class HandlerResultValueTaskFactory
{
    private static readonly Dictionary<HandlerResultStatus, HandlerResult> _resultCacheNoResponse = new()
    {
        { HandlerResultStatus.Success, new(HandlerResultStatus.Success) },
        { HandlerResultStatus.Skipped, new(HandlerResultStatus.Skipped) },
    };

    public static ValueTask<HandlerResult> SuccessfulResult() => ValueTask.FromResult(_resultCacheNoResponse[HandlerResultStatus.Success]);
    public static ValueTask<HandlerResult> Skipped() => ValueTask.FromResult(_resultCacheNoResponse[HandlerResultStatus.Skipped]);
    public static ValueTask<HandlerResult<TOut>> Skipped<TOut>()
        => ValueTask.FromResult(
            new HandlerResult<TOut>(HandlerResultStatus.Skipped));

    public static ValueTask<HandlerResult<TOut>> Success<TOut>(TOut value) => ValueTask.FromResult(
        new HandlerResult<TOut>(
            HandlerResultStatus.Success, value));

    public static ValueTask<HandlerResult<TOut>> Failed<TOut>(Exception err)
    {
        return ValueTask.FromResult(
            new HandlerResult<TOut>(
                HandlerResultStatus.Failed, err));
    }

    public static ValueTask<HandlerResult> FailedWithNullArgument(string argName)
    {
        return ValueTask.FromResult(
            new HandlerResult(
                HandlerResultStatus.Failed, new ArgumentNullException(argName)));
    }

    public static ValueTask<HandlerResult<TOut>> FailedWithNullArgument<TOut>(string argName) =>
        ValueTask.FromResult(
            new HandlerResult<TOut>(
                HandlerResultStatus.Failed, new ArgumentNullException(argName)));


    public static ValueTask<HandlerResult> FailedWithNullOrWhitespaceArgument(string argName)
        => ValueTask.FromResult(
            new HandlerResult(
                HandlerResultStatus.Failed, new ArgumentException("Argument is null or whitespace", argName)));

    public static ValueTask<HandlerResult> FailedWithNullOrEmptyArgument(string argName)
        => ValueTask.FromResult(
            new HandlerResult(
                HandlerResultStatus.Failed, new ArgumentException("Argument is null or empty", argName)));
}
