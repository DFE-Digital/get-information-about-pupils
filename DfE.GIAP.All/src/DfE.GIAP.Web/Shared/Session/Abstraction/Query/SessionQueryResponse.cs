namespace DfE.GIAP.Web.Shared.Session.Abstraction.Query;
#nullable enable
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
    public static SessionQueryResponse<TValue> CreateWithNoValue() => new(default, valueExists: false);
    public static SessionQueryResponse<TValue> Create(TValue value) => new(value, valueExists: true);
}
