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
    public HandlerResult(HandlerResultStatus status, Exception? exception = null) : base(status, exception)
    {

    }

    public HandlerResult(HandlerResultStatus status, TOut? result, Exception? exception = null) : this(status, exception)
    {
        Result = result;    
    }

    public TOut? Result { get; }
    
}

