namespace DfE.GIAP.Web.Features.Session;

public static class AspNetCoreSessionExtensions
{
    public static bool ContainsKey(this ISession session, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }
        return session.TryGetValue(key, out byte[] _);
    }
}
