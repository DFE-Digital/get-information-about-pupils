namespace DfE.GIAP.Web.Session.Abstraction.Query.Extensions;

internal static class SessionQueryResponseExtensions
{
    internal static TSessionObject TryGetValueOrDefaultWith<TSessionObject>(this SessionQueryResponse<TSessionObject> response, TSessionObject value)
        => TryGetValueOrDefaultWith(response, () => value);

    internal static TSessionObject TryGetValueOrDefaultWith<TSessionObject>(this SessionQueryResponse<TSessionObject> response, Func<TSessionObject> valueProvider)
    {
        return response.HasValue ?
            response.Value :
                valueProvider();
    }
}
