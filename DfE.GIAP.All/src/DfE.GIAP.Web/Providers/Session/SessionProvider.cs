using System.Text.Json;

namespace DfE.GIAP.Web.Providers.Session;

public class SessionProvider : ISessionProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionProvider(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("HttpContext or Session is not available. Make sure session middleware is properly configured.");

    public void SetSessionValue(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        Session.SetString(key, value);
    }

    public void SetSessionValue<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        string json = JsonSerializer.Serialize(value);
        Session.SetString(key, json);
    }

    public string GetSessionValue(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Session.GetString(key);
    }

    public T? GetSessionValueOrDefault<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        string json = GetSessionValue(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public void RemoveSessionValue(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        Session.Remove(key);
    }

    public bool ContainsSessionKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Session.Keys.Contains(key);
    }

    public void ClearSession()
    {
        List<string> keys = Session.Keys.ToList();
        foreach (string key in keys)
        {
            Session.Remove(key);
        }
    }
}
