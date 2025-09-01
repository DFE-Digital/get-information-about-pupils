namespace DfE.GIAP.Web.Session.Abstraction.Query;

public record SessionQueryResponse<TValue>
{
    private SessionQueryResponse(
        TValue? result,
        bool valueExists)
    {
        Value = result;
        HasValue = valueExists;
    }

    public TValue? Value { get; init; }
    public bool HasValue { get; init; }

    public static SessionQueryResponse<TValue> NoValue() => new(default(TValue), valueExists: false);
    public static SessionQueryResponse<TValue> Create(TValue value) => new(value, valueExists: true);
}
