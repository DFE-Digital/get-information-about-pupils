namespace DfE.GIAP.Web.Features.Session.Query;

#nullable enable
public sealed class SessionQueryResponse<TValue>
{
    public SessionQueryResponse(
        TValue? result,
        bool valueExists)
    {
        Value = result;
        HasValue = valueExists;
    }

    public TValue? Value { get; }
    public bool HasValue { get; }
}
